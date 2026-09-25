using System.Security.Claims;
using CareConnect.Data;
using CareConnect.Constants;
using CareConnect.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CareConnect.Areas.Customer.Controllers
{
    [Area("Customer")]
    [Authorize(Roles = "Khách hàng")]
    public class DashboardController : Controller
    {
        private readonly CareConnectDbContext _context;

        public DashboardController(
            CareConnectDbContext context)
        {
            _context = context;
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

            var khachHang = await _context.KhachHangs
                .AsNoTracking()
                .FirstOrDefaultAsync(k =>
                    k.MaTaiKhoan == maTaiKhoan);

            if (khachHang == null)
            {
                return RedirectToAction(
                    "AccessDenied",
                    "Account",
                    new { area = "" });
            }

            DateTime homNay = DateTime.Today;

            DateTime dauThang =
                new DateTime(
                    homNay.Year,
                    homNay.Month,
                    1);

            DateTime dauThangSau =
                dauThang.AddMonths(1);

            var model =
                new CustomerDashboardViewModel
                {
                    HoTenKhachHang =
                        khachHang.HoTen,

                    SoBuoiSapToi =
                        await _context.DatLichs.CountAsync(
                            d =>
                                d.MaKhachHang ==
                                    khachHang.MaKhachHang &&
                                d.NgayChamSoc >= homNay &&
                                d.TrangThai != BookingStatus.DaHuy &&
d.TrangThai != BookingStatus.DaHoanThanh),

                    SoHoSoDangQuanLy =
                        await _context.BenhNhans.CountAsync(
                            b =>
                                b.MaKhachHang ==
                                    khachHang.MaKhachHang),

                    SoBuoiHoanThanhThangNay =
                        await _context.DatLichs.CountAsync(
                            d =>
                                d.MaKhachHang ==
                                    khachHang.MaKhachHang &&
                                d.NgayChamSoc >= dauThang &&
                                d.NgayChamSoc < dauThangSau &&
                                d.TrangThai == "Hoàn thành"),

                    SoThongBaoMoi =
                        await _context.ThongBaos.CountAsync(
                            t =>
                                t.MaTaiKhoan == maTaiKhoan &&
                                !t.DaDoc)
                };

            model.LichSapToi =
                await _context.DatLichs
                    .AsNoTracking()
                    .Where(d =>
                        d.MaKhachHang ==
                            khachHang.MaKhachHang &&
                        d.NgayChamSoc >= homNay &&
                        d.TrangThai != BookingStatus.DaHuy &&
d.TrangThai != BookingStatus.DaHoanThanh)
                    .OrderBy(d => d.NgayChamSoc)
                    .ThenBy(d => d.GioBatDau)
                    .Take(3)
                    .Select(d =>
                        new UpcomingBookingViewModel
                        {
                            MaDatLich =
                                d.MaDatLich,

                            TenNguoiChamSoc =
                                d.NguoiChamSoc != null
                                    ? d.NguoiChamSoc.HoTen
                                        ?? "Chưa phân công"
                                    : "Chưa phân công",

                            TenBenhNhan =
                                d.BenhNhan != null
                                    ? d.BenhNhan.HoTen
                                    : string.Empty,

                            NgayChamSoc =
                                d.NgayChamSoc,

                            GioBatDau =
                                d.GioBatDau,

                            GioKetThuc =
                                d.GioKetThuc,

                            TongTien =
                                d.TongTien,

                            TrangThai =
                                d.TrangThai ?? string.Empty
                        })
                    .ToListAsync();

            model.ThongBaoGanDay =
                await _context.ThongBaos
                    .AsNoTracking()
                    .Where(t =>
                        t.MaTaiKhoan == maTaiKhoan)
                    .OrderByDescending(t => t.NgayGui)
                    .Take(4)
                    .Select(t =>
                        new CustomerNotificationViewModel
                        {
                            MaThongBao =
            t.MaThongBao,
                            TieuDe =
                                t.TieuDe ?? string.Empty,

                            NoiDung =
                                t.NoiDung ?? string.Empty,

                            NgayGui =
                                t.NgayGui,

                            DaDoc =
                                t.DaDoc
                        })
                    .ToListAsync();

            return View(model);
        }
    }
}