using System.Security.Claims;
using CareConnect.Data;
using CareConnect.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CareConnect.Areas.Caregiver.Controllers
{
    [Area("Caregiver")]
    [Authorize(Roles = "Người chăm sóc")]
    public class ProfileController : Controller
    {
        private readonly CareConnectDbContext _context;

        public ProfileController(CareConnectDbContext context)
        {
            _context = context;
        }

        // =====================================================
        // HIỂN THỊ HỒ SƠ
        // =====================================================
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            int? maTaiKhoan = LayMaTaiKhoan();

            if (!maTaiKhoan.HasValue)
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
                        x.MaTaiKhoan == maTaiKhoan.Value);

            if (nguoiChamSoc == null)
            {
                return NotFound(
                    "Không tìm thấy hồ sơ người chăm sóc.");
            }

            int soBuoiHoanThanh =
                await _context.DatLichs
                    .AsNoTracking()
                    .CountAsync(x =>
                        x.MaNguoiChamSoc
                            == nguoiChamSoc.MaNguoiChamSoc
                        && x.TrangThai == "Đã hoàn thành");

            int soDanhGia =
                await (
                    from danhGia in
                        _context.DanhGias.AsNoTracking()

                    join datLich in
                        _context.DatLichs.AsNoTracking()

                        on danhGia.MaDatLich
                        equals datLich.MaDatLich

                    where datLich.MaNguoiChamSoc
                        == nguoiChamSoc.MaNguoiChamSoc

                    select danhGia
                ).CountAsync();

            var model = new CaregiverProfileViewModel
            {
                MaNguoiChamSoc =
                    nguoiChamSoc.MaNguoiChamSoc,

                HoTen =
                    nguoiChamSoc.HoTen
                    ?? string.Empty,

                GioiTinh =
                    nguoiChamSoc.GioiTinh,

                NgaySinh =
                    nguoiChamSoc.NgaySinh,

                SoDienThoai =
                    nguoiChamSoc.SoDienThoai
                    ?? string.Empty,

                Email =
                    nguoiChamSoc.Email,

                DiaChi =
                    nguoiChamSoc.DiaChi,

                BangCap =
                    nguoiChamSoc.BangCap,

                KinhNghiem =
                    nguoiChamSoc.KinhNghiem
                    ?? 0,

                ChuyenMon =
                    nguoiChamSoc.ChuyenMon
                    ?? string.Empty,

                KhuVucHoatDong =
                    nguoiChamSoc.KhuVucHoatDong
                    ?? string.Empty,

                // Không đổi null thành 0
                GiaTheoGio =
                    nguoiChamSoc.GiaTheoGio,

                GioiThieu =
                    nguoiChamSoc.GioiThieu
                    ?? string.Empty,

                DanhGia =
                    nguoiChamSoc.DanhGia
                    ?? 0,

                TrangThai =
                    nguoiChamSoc.TrangThai
                    ?? "Chưa cập nhật",

                NgayTao =
                    nguoiChamSoc.NgayTao,

                SoBuoiHoanThanh =
                    soBuoiHoanThanh,

                SoDanhGia =
                    soDanhGia
            };

            return View(model);
        }

        // =====================================================
        // CẬP NHẬT HỒ SƠ
        // =====================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(
            CaregiverProfileViewModel model)
        {
            int? maTaiKhoan = LayMaTaiKhoan();

            if (!maTaiKhoan.HasValue)
            {
                return Unauthorized();
            }

            var nguoiChamSoc =
                await _context.NguoiChamSocs
                    .FirstOrDefaultAsync(x =>
                        x.MaTaiKhoan == maTaiKhoan.Value);

            if (nguoiChamSoc == null)
            {
                return NotFound(
                    "Không tìm thấy hồ sơ người chăm sóc.");
            }

            /*
             * Các trường chỉ dùng hiển thị.
             * Không kiểm tra validation khi người dùng cập nhật.
             */
            ModelState.Remove(
                nameof(CaregiverProfileViewModel.MaNguoiChamSoc));

            ModelState.Remove(
                nameof(CaregiverProfileViewModel.TrangThai));

            ModelState.Remove(
                nameof(CaregiverProfileViewModel.NgayTao));

            ModelState.Remove(
                nameof(CaregiverProfileViewModel.DanhGia));

            ModelState.Remove(
                nameof(CaregiverProfileViewModel.SoDanhGia));

            ModelState.Remove(
                nameof(CaregiverProfileViewModel.SoBuoiHoanThanh));

            /*
             * Giá theo giờ được phép để trống.
             * Không để giá ngăn các trường khác được lưu.
             */
            ModelState.Remove(
                nameof(CaregiverProfileViewModel.GiaTheoGio));

            if (!ModelState.IsValid)
            {
                string thongBaoLoi =
                    string.Join(
                        " | ",
                        ModelState.Values
                            .SelectMany(x => x.Errors)
                            .Select(x =>
                                string.IsNullOrWhiteSpace(
                                    x.ErrorMessage)
                                    ? "Thông tin chưa hợp lệ."
                                    : x.ErrorMessage));

                TempData["Error"] =
                    string.IsNullOrWhiteSpace(thongBaoLoi)
                        ? "Vui lòng kiểm tra lại thông tin hồ sơ."
                        : thongBaoLoi;

                return RedirectToAction(nameof(Index));
            }

            // =================================================
            // GÁN DỮ LIỆU MỚI VÀO ENTITY ĐANG ĐƯỢC EF THEO DÕI
            // =================================================

            nguoiChamSoc.HoTen =
                model.HoTen?.Trim()
                ?? string.Empty;

            nguoiChamSoc.GioiTinh =
                string.IsNullOrWhiteSpace(model.GioiTinh)
                    ? null
                    : model.GioiTinh.Trim();

            nguoiChamSoc.NgaySinh =
                model.NgaySinh;

            nguoiChamSoc.SoDienThoai =
                model.SoDienThoai?.Trim()
                ?? string.Empty;

            nguoiChamSoc.Email =
                string.IsNullOrWhiteSpace(model.Email)
                    ? null
                    : model.Email.Trim();

            nguoiChamSoc.DiaChi =
                string.IsNullOrWhiteSpace(model.DiaChi)
                    ? null
                    : model.DiaChi.Trim();

            nguoiChamSoc.BangCap =
                string.IsNullOrWhiteSpace(model.BangCap)
                    ? null
                    : model.BangCap.Trim();

            nguoiChamSoc.KinhNghiem =
                model.KinhNghiem;

            nguoiChamSoc.ChuyenMon =
                model.ChuyenMon?.Trim()
                ?? string.Empty;

            nguoiChamSoc.KhuVucHoatDong =
                model.KhuVucHoatDong?.Trim()
                ?? string.Empty;

            /*
             * Có nhập giá mới thì cập nhật.
             * Để trống thì giữ nguyên giá cũ trong SQL.
             */
            if (model.GiaTheoGio.HasValue)
            {
                nguoiChamSoc.GiaTheoGio =
                    model.GiaTheoGio.Value;
            }

            nguoiChamSoc.GioiThieu =
                model.GioiThieu?.Trim()
                ?? string.Empty;

            try
            {
                int soDongDaCapNhat =
                    await _context.SaveChangesAsync();

                if (soDongDaCapNhat > 0)
                {
                    TempData["Success"] =
                        "Hồ sơ cá nhân của bạn đã được cập nhật thành công.";
                }
                else
                {
                    TempData["Error"] =
                        "Không có thông tin nào thay đổi.";
                }
            }
            catch (DbUpdateException)
            {
                TempData["Error"] =
                    "Không thể lưu thông tin vào cơ sở dữ liệu. "
                    + "Vui lòng kiểm tra lại dữ liệu.";
            }
            catch (Exception)
            {
                TempData["Error"] =
                    "Đã xảy ra lỗi khi cập nhật hồ sơ.";
            }

            return RedirectToAction(nameof(Index));
        }

        // =====================================================
        // LẤY MÃ TÀI KHOẢN ĐANG ĐĂNG NHẬP
        // =====================================================
        private int? LayMaTaiKhoan()
        {
            string? claim =
                User.FindFirstValue(
                    ClaimTypes.NameIdentifier);

            return int.TryParse(
                claim,
                out int maTaiKhoan)
                    ? maTaiKhoan
                    : null;
        }
    }
}