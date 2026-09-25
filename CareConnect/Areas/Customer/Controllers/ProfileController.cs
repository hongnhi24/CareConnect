using System.Security.Claims;
using CareConnect.Data;
using CareConnect.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CareConnect.Areas.Customer.Controllers
{
    [Area("Customer")]
    [Authorize(Roles = "Khách hàng")]
    public class ProfileController : Controller
    {
        private readonly CareConnectDbContext _context;

        public ProfileController(
            CareConnectDbContext context)
        {
            _context = context;
        }

        private int? LayMaTaiKhoan()
        {
            string? claim =
                User.FindFirstValue(
                    ClaimTypes.NameIdentifier);

            if (!int.TryParse(
                claim,
                out int maTaiKhoan))
            {
                return null;
            }

            return maTaiKhoan;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            int? maTaiKhoan =
                LayMaTaiKhoan();

            if (!maTaiKhoan.HasValue)
            {
                return Forbid();
            }

            var khachHang =
                await _context.KhachHangs
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x =>
                        x.MaTaiKhoan ==
                            maTaiKhoan.Value
                        &&
                        x.TrangThaiHoatDong);

            if (khachHang == null)
            {
                return NotFound();
            }

            var model =
                new CustomerProfileViewModel
                {
                    MaKhachHang =
                        khachHang.MaKhachHang,

                    HoTen =
                        khachHang.HoTen ?? string.Empty,

                    GioiTinh =
                        khachHang.GioiTinh,

                    NgaySinh =
                        khachHang.NgaySinh,

                    SoDienThoai =
                        khachHang.SoDienThoai
                        ?? string.Empty,

                    Email =
                        khachHang.Email,

                    DiaChi =
                        khachHang.DiaChi,

                    CCCD =
                        khachHang.CCCD,

                    SoNguoiThan =
                        await _context.BenhNhans
                            .CountAsync(x =>
                                x.MaKhachHang ==
                                    khachHang.MaKhachHang
                                &&
                                x.TrangThaiHoatDong),

                    SoLichChamSoc =
                        await _context.DatLichs
                            .CountAsync(x =>
                                x.MaKhachHang ==
                                    khachHang.MaKhachHang)
                };

            return View(model);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteAccount()
        {
            int? maTaiKhoan =
                LayMaTaiKhoan();

            if (!maTaiKhoan.HasValue)
            {
                return Forbid();
            }

            var taiKhoan =
                await _context.TaiKhoans
                    .FirstOrDefaultAsync(x =>
                        x.MaTaiKhoan ==
                            maTaiKhoan.Value);

            if (taiKhoan == null)
            {
                return NotFound();
            }

            var khachHang =
                await _context.KhachHangs
                    .FirstOrDefaultAsync(x =>
                        x.MaTaiKhoan ==
                            maTaiKhoan.Value
                        &&
                        x.TrangThaiHoatDong);

            if (khachHang == null)
            {
                TempData["Error"] =
                    "Không tìm thấy thông tin khách hàng.";

                return RedirectToAction(nameof(Index));
            }

            /*
             * Không cho xóa tài khoản khi đang có
             * một buổi chăm sóc thực tế đang diễn ra.
             */
            bool coLichDangThucHien =
                await _context.DatLichs
                    .AnyAsync(x =>
                        x.MaKhachHang ==
                            khachHang.MaKhachHang
                        &&
                        x.TrangThai ==
                            "Đang thực hiện");

            if (coLichDangThucHien)
            {
                TempData["Error"] =
                    "Không thể xóa tài khoản vì đang có "
                    + "lịch chăm sóc đang thực hiện.";

                return RedirectToAction(nameof(Index));
            }

            await using var transaction =
                await _context.Database
                    .BeginTransactionAsync();

            try
            {
                DateTime thoiDiemXoa =
                    DateTime.Now;

                /*
                 * 1. Hủy các lịch chưa hoàn thành.
                 */
                var lichCanHuy =
                    await _context.DatLichs
                        .Where(x =>
                            x.MaKhachHang ==
                                khachHang.MaKhachHang
                            &&
                            (
                                x.TrangThai ==
                                    "Chờ xác nhận"
                                ||
                                x.TrangThai ==
                                    "Đã xác nhận"
                            ))
                        .ToListAsync();

                foreach (var lich in lichCanHuy)
                {
                    lich.TrangThai =
                        "Đã hủy";

                    lich.LyDoHuy =
                        "Khách hàng đã xóa tài khoản.";

                    lich.NguoiHuy =
                        "Khách hàng";

                    lich.NgayHuy =
                        thoiDiemXoa;
                }

                /*
                 * 2. Vô hiệu hóa toàn bộ hồ sơ người thân.
                 */
                var danhSachBenhNhan =
                    await _context.BenhNhans
                        .Where(x =>
                            x.MaKhachHang ==
                                khachHang.MaKhachHang
                            &&
                            x.TrangThaiHoatDong)
                        .ToListAsync();

                foreach (var benhNhan
                         in danhSachBenhNhan)
                {
                    benhNhan.TrangThaiHoatDong =
                        false;

                    benhNhan.NgayXoa =
                        thoiDiemXoa;
                }

                /*
                 * 3. Vô hiệu hóa hồ sơ khách hàng.
                 */
                khachHang.TrangThaiHoatDong =
                    false;

                khachHang.NgayXoa =
                    thoiDiemXoa;

                /*
                 * 4. Khóa tài khoản đăng nhập.
                 */
                taiKhoan.TrangThai =
                    false;

                await _context.SaveChangesAsync();

                await transaction.CommitAsync();

                /*
                 * Sau khi xóa tài khoản,
                 * không giữ phiên đăng nhập nữa.
                 */
                return RedirectToAction(
                    "LogoutAfterDelete",
                    "Account",
                    new { area = "" });
            }
            catch
            {
                await transaction.RollbackAsync();

                TempData["Error"] =
                    "Không thể xóa tài khoản. "
                    + "Dữ liệu chưa bị thay đổi.";

                return RedirectToAction(nameof(Index));
            }
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(
            CustomerProfileViewModel model)
        {
            int? maTaiKhoan =
                LayMaTaiKhoan();

            if (!maTaiKhoan.HasValue)
            {
                return Forbid();
            }

            var khachHang =
                await _context.KhachHangs
                    .FirstOrDefaultAsync(x =>
                        x.MaTaiKhoan ==
                            maTaiKhoan.Value
                        &&
                        x.TrangThaiHoatDong);

            if (khachHang == null)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                model.SoNguoiThan =
                    await _context.BenhNhans
                        .CountAsync(x =>
                            x.MaKhachHang ==
                                khachHang.MaKhachHang
                            &&
                            x.TrangThaiHoatDong);

                model.SoLichChamSoc =
                    await _context.DatLichs
                        .CountAsync(x =>
                            x.MaKhachHang ==
                                khachHang.MaKhachHang);

                return View(model);
            }

            khachHang.HoTen =
                model.HoTen.Trim();

            khachHang.GioiTinh =
                model.GioiTinh;

            khachHang.NgaySinh =
                model.NgaySinh;

            khachHang.SoDienThoai =
                model.SoDienThoai.Trim();

            khachHang.Email =
                model.Email?.Trim();

            khachHang.DiaChi =
                model.DiaChi?.Trim();

            khachHang.CCCD =
                model.CCCD?.Trim();

            await _context.SaveChangesAsync();

            TempData["Success"] =
                "Cập nhật thông tin cá nhân thành công.";

            return RedirectToAction(nameof(Index));
        }
    }
}