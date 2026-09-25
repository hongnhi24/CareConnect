using CareConnect.Constants;
using CareConnect.Data;
using CareConnect.Services;
using CareConnect.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace CareConnect.Areas.Caregiver.Controllers
{
    [Area("Caregiver")]
    [Authorize(Roles = "Người chăm sóc")]
    public class DashboardController : Controller
    {
        private readonly CareConnectDbContext _context;

        private readonly
    ICaregiverProfileCompletenessService
        _profileCompletenessService;

        public DashboardController(
    CareConnectDbContext context,
    ICaregiverProfileCompletenessService
        profileCompletenessService)
        {
            _context =
                context;

            _profileCompletenessService =
                profileCompletenessService;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            string? maTaiKhoanClaim =
                User.FindFirstValue(
                    ClaimTypes.NameIdentifier);

            if (!int.TryParse(
                    maTaiKhoanClaim,
                    out int maTaiKhoan))
            {
                return RedirectToAction(
                    "Login",
                    "Account",
                    new { area = "" });
            }

            var nguoiChamSoc =
                await _context.NguoiChamSocs
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x =>
                        x.MaTaiKhoan == maTaiKhoan);

            if (nguoiChamSoc == null)
            {
                return View(
                    new CaregiverDashboardViewModel
                    {
                        HoTen =
                            User.Identity?.Name
                            ?? "Người chăm sóc"
                    });
            }

            var tinhTrangHoSo =
                await _profileCompletenessService
                    .KiemTraAsync(
                        nguoiChamSoc.MaNguoiChamSoc);

            ViewBag.ProfileCompleteness =
                tinhTrangHoSo;

            DateTime homNay = DateTime.Today;

            DateTime dauThang =
                new DateTime(
                    homNay.Year,
                    homNay.Month,
                    1);

            DateTime dauThangSau =
                dauThang.AddMonths(1);

            int maNguoiChamSoc =
                nguoiChamSoc.MaNguoiChamSoc;

            var lichCuaNguoiChamSoc =
                _context.DatLichs
                    .AsNoTracking()
                    .Where(x =>
                        x.MaNguoiChamSoc
                            == maNguoiChamSoc);

            int soBuoiHomNay =
                await lichCuaNguoiChamSoc
                    .CountAsync(x =>
                        x.NgayChamSoc.HasValue
                        && x.NgayChamSoc.Value.Date
                            == homNay
                        && x.TrangThai != "Đã hủy");

            int soYeuCauMoi =
                await lichCuaNguoiChamSoc
                    .CountAsync(x =>
                        x.TrangThai == BookingStatus.ChoXacNhan
                        && x.NgayChamSoc.HasValue
                        && x.NgayChamSoc.Value.Date
                            >= homNay);

            int soBuoiHoanThanh =
                await lichCuaNguoiChamSoc
                    .CountAsync(x =>
                        x.TrangThai == BookingStatus.DaHoanThanh
                        && x.NgayChamSoc.HasValue
                        && x.NgayChamSoc.Value
                            >= dauThang
                        && x.NgayChamSoc.Value
                            < dauThangSau);

            decimal thuNhapThang =
                await lichCuaNguoiChamSoc
                    .Where(x =>
                        x.TrangThai == BookingStatus.DaHoanThanh
                        && x.NgayChamSoc.HasValue
                        && x.NgayChamSoc.Value
                            >= dauThang
                        && x.NgayChamSoc.Value
                            < dauThangSau)
                    .SumAsync(x =>
                        x.ThuNhapNguoiChamSoc ?? 0);

            int soThongBaoChuaDoc =
                await _context.ThongBaos
                    .AsNoTracking()
                    .CountAsync(x =>
                        x.MaTaiKhoan == maTaiKhoan
                        && !x.DaDoc);

            var lichSapToi =
                await lichCuaNguoiChamSoc
                    .Where(x =>
                        x.NgayChamSoc.HasValue
                        && x.NgayChamSoc.Value.Date
                            >= homNay
                        && x.TrangThai
                            != "Đã hủy"
                        && x.TrangThai
                            != "Đã hoàn thành")
                    .OrderBy(x => x.NgayChamSoc)
                    .ThenBy(x => x.GioBatDau)
                    .Take(5)
                    .Select(x =>
                        new CaregiverScheduleItemViewModel
                        {
                            MaDatLich =
                                x.MaDatLich,

                            TenBenhNhan =
                                x.BenhNhan != null
                                    ? x.BenhNhan.HoTen
                                    : "Chưa cập nhật",

                            TenKhachHang =
                                x.KhachHang != null
                                    ? x.KhachHang.HoTen
                                    : "Chưa cập nhật",

                            NgayChamSoc =
                                x.NgayChamSoc
                                ?? homNay,

                            GioBatDau =
                                x.GioBatDau
                                ?? TimeSpan.Zero,

                            GioKetThuc =
                                x.GioKetThuc
                                ?? TimeSpan.Zero,

                            DiaChiChamSoc =
                                x.DiaChiChamSoc
                                ?? "Chưa cập nhật",

                            TrangThai =
                                x.TrangThai
                                ?? "Chưa xác định",

                            TongTien =
                                x.TongTien ?? 0
                        })
                    .ToListAsync();

            var yeuCauMoi =
                await lichCuaNguoiChamSoc
                    .Where(x =>
                        x.TrangThai
                            == BookingStatus.ChoXacNhan
                        && x.NgayChamSoc.HasValue
                        && x.NgayChamSoc.Value.Date
                            >= homNay)
                    .OrderBy(x => x.NgayChamSoc)
                    .ThenBy(x => x.GioBatDau)
                    .Take(3)
                    .Select(x =>
                        new CaregiverRequestItemViewModel
                        {
                            MaDatLich =
                                x.MaDatLich,

                            TenBenhNhan =
                                x.BenhNhan != null
                                    ? x.BenhNhan.HoTen
                                    : "Chưa cập nhật",

                            TenKhachHang =
                                x.KhachHang != null
                                    ? x.KhachHang.HoTen
                                    : "Chưa cập nhật",

                            NgayChamSoc =
                                x.NgayChamSoc
                                ?? homNay,

                            GioBatDau =
                                x.GioBatDau
                                ?? TimeSpan.Zero,

                            GioKetThuc =
                                x.GioKetThuc
                                ?? TimeSpan.Zero,

                            DiaChiChamSoc =
                                x.DiaChiChamSoc
                                ?? "Chưa cập nhật",

                            TongTien =
                                x.TongTien ?? 0
                        })
                    .ToListAsync();

            var thongBaoGanDay =
                await _context.ThongBaos
                    .AsNoTracking()
                    .Where(x =>
                        x.MaTaiKhoan == maTaiKhoan)
                    .OrderByDescending(x => x.NgayGui)
                    .Take(4)
                    .Select(x =>
                        new CaregiverNotificationItemViewModel
                        {
                            TieuDe =
                                x.TieuDe
                                ?? "Thông báo",

                            NoiDung =
                                x.NoiDung
                                ?? string.Empty,

                            NgayGui =
                                x.NgayGui,

                            DaDoc =
                                x.DaDoc
                        })
                    .ToListAsync();

            var model =
                new CaregiverDashboardViewModel
                {
                    HoTen =
                        nguoiChamSoc.HoTen
                        ?? User.Identity?.Name
                        ?? "Người chăm sóc",

                    ChuyenMon =
                        nguoiChamSoc.ChuyenMon
                        ?? "Chăm sóc người cao tuổi",

                    KhuVucHoatDong =
                        nguoiChamSoc.KhuVucHoatDong
                        ?? "Chưa cập nhật",

                    TrangThaiHoSo =
                        nguoiChamSoc.TrangThai
                        ?? "Chưa cập nhật",

                    KinhNghiem =
                        nguoiChamSoc.KinhNghiem
                        ?? 0,

                    DanhGia =
                        nguoiChamSoc.DanhGia
                        ?? 0,

                    GiaTheoGio =
                        nguoiChamSoc.GiaTheoGio
                        ?? 0,

                    SoBuoiHomNay =
                        soBuoiHomNay,

                    SoYeuCauMoi =
                        soYeuCauMoi,

                    SoBuoiHoanThanhThangNay =
                        soBuoiHoanThanh,

                    ThuNhapThangNay =
                        thuNhapThang,

                    SoThongBaoChuaDoc =
                        soThongBaoChuaDoc,

                    LichSapToi =
                        lichSapToi,

                    YeuCauChoXacNhan =
                        yeuCauMoi,

                    ThongBaoGanDay =
                        thongBaoGanDay
                };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> XacNhanLich(
            int maDatLich)
        {
            string? maTaiKhoanClaim =
                User.FindFirstValue(
                    ClaimTypes.NameIdentifier);

            if (!int.TryParse(
                    maTaiKhoanClaim,
                    out int maTaiKhoan))
            {
                return Unauthorized();
            }

            var nguoiChamSoc =
                await _context.NguoiChamSocs
                    .FirstOrDefaultAsync(x =>
                        x.MaTaiKhoan == maTaiKhoan);

            if (nguoiChamSoc == null)
            {
                return NotFound();
            }

            var datLich =
                await _context.DatLichs
                    .FirstOrDefaultAsync(x =>
                        x.MaDatLich == maDatLich
                        && x.MaNguoiChamSoc
                            == nguoiChamSoc.MaNguoiChamSoc);

            if (datLich == null)
            {
                TempData["Error"] =
                    "Không tìm thấy yêu cầu đặt lịch.";

                return RedirectToAction(nameof(Index));
            }

            if (datLich.TrangThai != "Chờ xác nhận")
            {
                TempData["Error"] =
                    "Yêu cầu này đã được xử lý.";

                return RedirectToAction(nameof(Index));
            }

            bool trungLich =
                await _context.DatLichs
                    .AnyAsync(x =>
                        x.MaDatLich
                            != datLich.MaDatLich
                        && x.MaNguoiChamSoc
                            == nguoiChamSoc.MaNguoiChamSoc
                        && x.NgayChamSoc
                            == datLich.NgayChamSoc
                        && x.TrangThai
                            != "Đã hủy"
                        && x.GioBatDau
                            < datLich.GioKetThuc
                        && x.GioKetThuc
                            > datLich.GioBatDau);

            if (trungLich)
            {
                TempData["Error"] =
                    "Lịch này bị trùng với lịch làm việc hiện có.";

                return RedirectToAction(nameof(Index));
            }

            datLich.TrangThai =
                "Đã xác nhận";

            await _context.SaveChangesAsync();

            TempData["Success"] =
                "Xác nhận lịch chăm sóc thành công.";

            return RedirectToAction(nameof(Index));
        }
    }
}