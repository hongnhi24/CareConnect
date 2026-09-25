using CareConnect.Data;
using CareConnect.Models;
using CareConnect.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CareConnect.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Quản trị viên")]
    public class CaregiversController : Controller
    {
        private readonly CareConnectDbContext _context;

        public CaregiversController(
            CareConnectDbContext context)
        {
            _context = context;
        }

        // DANH SÁCH HỒ SƠ
        [HttpGet]
        public async Task<IActionResult> Index(
            string? tuKhoa,
            string? trangThai)
        {
            tuKhoa = tuKhoa?.Trim() ?? string.Empty;
            trangThai = trangThai?.Trim() ?? string.Empty;

            var query = _context.NguoiChamSocs
                .AsNoTracking()
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(tuKhoa))
            {
                query = query.Where(x =>
                    (x.HoTen != null &&
                     x.HoTen.Contains(tuKhoa))
                    ||
                    (x.Email != null &&
                     x.Email.Contains(tuKhoa))
                    ||
                    (x.SoDienThoai != null &&
                     x.SoDienThoai.Contains(tuKhoa))
                    ||
                    (x.ChuyenMon != null &&
                     x.ChuyenMon.Contains(tuKhoa)));
            }

            if (!string.IsNullOrWhiteSpace(trangThai))
            {
                query = query.Where(x =>
                    x.TrangThai == trangThai);
            }

            var danhSach = await query
                .OrderBy(x =>
                    x.TrangThai == "Chờ duyệt" ? 0 : 1)
                .ThenByDescending(x => x.NgayTao)
                .Select(x =>
                    new AdminCaregiverListItemViewModel
                    {
                        MaNguoiChamSoc =
                            x.MaNguoiChamSoc,

                        HoTen =
                            x.HoTen ?? "Chưa cập nhật",

                        Email =
                            x.Email ?? "Chưa cập nhật",

                        SoDienThoai =
                            x.SoDienThoai ?? "Chưa cập nhật",

                        ChuyenMon =
                            x.ChuyenMon ?? "Chưa cập nhật",

                        KhuVucHoatDong =
                            x.KhuVucHoatDong
                            ?? "Chưa cập nhật",

                        KinhNghiem =
                            x.KinhNghiem ?? 0,

                        GiaTheoGio =
                            x.GiaTheoGio ?? 0,

                        TrangThai =
                            x.TrangThai ?? "Chưa xác định",

                        NgayTao =
                            x.NgayTao
                    })
                .ToListAsync();

            var model = new AdminCaregiverListViewModel
            {
                TongHoSo =
                    await _context.NguoiChamSocs
                        .AsNoTracking()
                        .CountAsync(),

                ChoDuyet =
                    await _context.NguoiChamSocs
                        .AsNoTracking()
                        .CountAsync(x =>
                            x.TrangThai == "Chờ duyệt"),

                DaDuyet =
                    await _context.NguoiChamSocs
                        .AsNoTracking()
                        .CountAsync(x =>
                            x.TrangThai == "Đã duyệt"),

                TuChoi =
                    await _context.NguoiChamSocs
                        .AsNoTracking()
                        .CountAsync(x =>
                            x.TrangThai == "Từ chối"),

                TuKhoa = tuKhoa,
                TrangThai = trangThai,
                DanhSach = danhSach
            };

            return View(model);
        }

        // XEM CHI TIẾT HỒ SƠ
        [HttpGet]
        public async Task<IActionResult> Review(
            int id)
        {
            var nguoiChamSoc =
                await _context.NguoiChamSocs
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x =>
                        x.MaNguoiChamSoc == id);

            if (nguoiChamSoc == null)
            {
                return NotFound();
            }

            TaiKhoan? taiKhoan = null;

            if (nguoiChamSoc.MaTaiKhoan != null)
            {
                taiKhoan =
                    await _context.TaiKhoans
                        .AsNoTracking()
                        .FirstOrDefaultAsync(x =>
                            x.MaTaiKhoan ==
                            nguoiChamSoc.MaTaiKhoan);
            }

            var model =
                new AdminCaregiverReviewViewModel
                {
                    MaNguoiChamSoc =
                        nguoiChamSoc.MaNguoiChamSoc,

                    MaTaiKhoan =
                        nguoiChamSoc.MaTaiKhoan,

                    HoTen =
                        nguoiChamSoc.HoTen
                        ?? "Chưa cập nhật",

                    Email =
                        nguoiChamSoc.Email
                        ?? taiKhoan?.Email
                        ?? "Chưa cập nhật",

                    SoDienThoai =
                        nguoiChamSoc.SoDienThoai
                        ?? "Chưa cập nhật",

                    BangCap =
                        nguoiChamSoc.BangCap
                        ?? "Chưa cập nhật",

                    ChuyenMon =
                        nguoiChamSoc.ChuyenMon
                        ?? "Chưa cập nhật",

                    KinhNghiem =
                        nguoiChamSoc.KinhNghiem
                        ?? 0,

                    KhuVucHoatDong =
                        nguoiChamSoc.KhuVucHoatDong
                        ?? "Chưa cập nhật",

                    GiaTheoGio =
                        nguoiChamSoc.GiaTheoGio
                        ?? 0,

                    DanhGia =
                        nguoiChamSoc.DanhGia
                        ?? 0,

                    GioiThieu =
                        nguoiChamSoc.GioiThieu
                        ?? "Chưa cập nhật",

                    TrangThai =
                        nguoiChamSoc.TrangThai
                        ?? "Chưa xác định",

                    NgayTao =
                        nguoiChamSoc.NgayTao,

                    TenDangNhap =
                        taiKhoan?.TenDangNhap
                        ?? "Chưa cập nhật",

                    TrangThaiTaiKhoan =
                        taiKhoan?.TrangThai
                        ?? false
                };

            return View(model);
        }

        // DUYỆT HỒ SƠ
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Approve(
            int maNguoiChamSoc)
        {
            var nguoiChamSoc =
                await _context.NguoiChamSocs
                    .FirstOrDefaultAsync(x =>
                        x.MaNguoiChamSoc ==
                        maNguoiChamSoc);

            if (nguoiChamSoc == null)
            {
                TempData["Error"] =
                    "Không tìm thấy hồ sơ người chăm sóc.";

                return RedirectToAction(nameof(Index));
            }

            if (nguoiChamSoc.TrangThai == "Đã duyệt")
            {
                TempData["Error"] =
                    "Hồ sơ này đã được duyệt trước đó.";

                return RedirectToAction(
                    nameof(Review),
                    new { id = maNguoiChamSoc });
            }

            nguoiChamSoc.TrangThai =
                "Đã duyệt";

            TaiKhoan? taiKhoan = null;

            if (nguoiChamSoc.MaTaiKhoan != null)
            {
                taiKhoan =
                    await _context.TaiKhoans
                        .FirstOrDefaultAsync(x =>
                            x.MaTaiKhoan ==
                            nguoiChamSoc.MaTaiKhoan);

                if (taiKhoan != null)
                {
                    taiKhoan.TrangThai = true;
                }
            }

            // Tạo thông báo cho người chăm sóc
            if (nguoiChamSoc.MaTaiKhoan != null)
            {
                var thongBao = new ThongBao
                {
                    MaTaiKhoan =
                        nguoiChamSoc.MaTaiKhoan.Value,

                    TieuDe =
                        "Hồ sơ đã được phê duyệt",

                    NoiDung =
                        "Hồ sơ người chăm sóc của bạn "
                        + "đã được quản trị viên phê duyệt. "
                        + "Bạn có thể sử dụng đầy đủ "
                        + "các chức năng của hệ thống.",

                    NgayGui =
                        DateTime.Now,

                    DaDoc =
                        false
                };

                _context.ThongBaos.Add(thongBao);
            }

            await _context.SaveChangesAsync();

            TempData["Success"] =
                $"Đã phê duyệt hồ sơ của "
                + $"{nguoiChamSoc.HoTen}.";

            return RedirectToAction(
                nameof(Review),
                new { id = maNguoiChamSoc });
        }

        // TỪ CHỐI HỒ SƠ
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reject(
            RejectCaregiverViewModel model)
        {
            if (string.IsNullOrWhiteSpace(model.LyDo))
            {
                TempData["Error"] =
                    "Vui lòng nhập lý do từ chối hồ sơ.";

                return RedirectToAction(
                    nameof(Review),
                    new { id = model.MaNguoiChamSoc });
            }

            var nguoiChamSoc =
                await _context.NguoiChamSocs
                    .FirstOrDefaultAsync(x =>
                        x.MaNguoiChamSoc ==
                        model.MaNguoiChamSoc);

            if (nguoiChamSoc == null)
            {
                TempData["Error"] =
                    "Không tìm thấy hồ sơ người chăm sóc.";

                return RedirectToAction(nameof(Index));
            }

            nguoiChamSoc.TrangThai =
                "Từ chối";

            /*
             * Không khóa TaiKhoan ở đây.
             * Người chăm sóc vẫn phải đăng nhập được
             * để xem lý do và cập nhật lại hồ sơ.
             */
            if (nguoiChamSoc.MaTaiKhoan != null)
            {
                var taiKhoan =
                    await _context.TaiKhoans
                        .FirstOrDefaultAsync(x =>
                            x.MaTaiKhoan ==
                            nguoiChamSoc.MaTaiKhoan);

                if (taiKhoan != null)
                {
                    taiKhoan.TrangThai = true;
                }

                var thongBao = new ThongBao
                {
                    MaTaiKhoan =
                        nguoiChamSoc.MaTaiKhoan.Value,

                    TieuDe =
                        "Hồ sơ cần được cập nhật",

                    NoiDung =
                        "Hồ sơ của bạn chưa được phê duyệt. "
                        + "Lý do: "
                        + model.LyDo.Trim(),

                    NgayGui =
                        DateTime.Now,

                    DaDoc =
                        false
                };

                _context.ThongBaos.Add(thongBao);
            }

            await _context.SaveChangesAsync();

            TempData["Success"] =
                $"Đã từ chối hồ sơ của "
                + $"{nguoiChamSoc.HoTen}.";

            return RedirectToAction(nameof(Index));
        }
    }
}