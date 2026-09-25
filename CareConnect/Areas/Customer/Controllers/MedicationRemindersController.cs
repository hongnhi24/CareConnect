using System.Security.Claims;
using CareConnect.Data;
using CareConnect.Models;
using CareConnect.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CareConnect.Areas.Customer.Controllers
{
    [Area("Customer")]
    [Authorize(Roles = "Khách hàng")]
    public class MedicationRemindersController : Controller
    {
        private readonly CareConnectDbContext _context;

        public MedicationRemindersController(
            CareConnectDbContext context)
        {
            _context = context;
        }


        // ============================================
        // LẤY KHÁCH HÀNG ĐANG ĐĂNG NHẬP
        // ============================================
        private async Task<int?>
            LayMaKhachHangDangNhap()
        {
            string? maTaiKhoanClaim =
                User.FindFirstValue(
                    ClaimTypes.NameIdentifier);

            if (!int.TryParse(
                maTaiKhoanClaim,
                out int maTaiKhoan))
            {
                return null;
            }

            return await _context.KhachHangs
                .AsNoTracking()
                .Where(x =>
                    x.MaTaiKhoan == maTaiKhoan
                    &&
                    x.TrangThaiHoatDong)
                .Select(x =>
                    (int?)x.MaKhachHang)
                .FirstOrDefaultAsync();
        }


        // ============================================
        // TRANG DANH SÁCH LỊCH NHẮC
        // ============================================
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            int? maKhachHang =
                await LayMaKhachHangDangNhap();

            if (!maKhachHang.HasValue)
            {
                return Forbid();
            }

            var model =
                await TaoPageModel(
                    maKhachHang.Value);

            model.Form.NgayBatDau =
                DateTime.Today;

            model.Form.TanSuat =
                "Hằng ngày";

            return View(model);
        }


        // ============================================
        // TẠO LỊCH NHẮC
        // ============================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            [Bind(Prefix = "Form")]
            CustomerMedicationReminderFormViewModel form)
        {
            int? maKhachHang =
                await LayMaKhachHangDangNhap();

            if (!maKhachHang.HasValue)
            {
                return Forbid();
            }

            // Kiểm tra ngày
            if (
                form.NgayBatDau.HasValue
                &&
                form.NgayKetThuc.HasValue
                &&
                form.NgayKetThuc.Value.Date
                    < form.NgayBatDau.Value.Date)
            {
                ModelState.AddModelError(
                    "Form.NgayKetThuc",
                    "Ngày kết thúc phải lớn hơn hoặc bằng ngày bắt đầu.");
            }


            // Kiểm tra bệnh nhân có thuộc
            // khách hàng đang đăng nhập không
            bool benhNhanHopLe =
                form.MaBenhNhan.HasValue
                &&
                await _context.BenhNhans
                    .AsNoTracking()
                    .AnyAsync(x =>
                        x.MaBenhNhan
                            == form.MaBenhNhan.Value
                        &&
                        x.MaKhachHang
                            == maKhachHang.Value
                        &&
                        x.TrangThaiHoatDong);

            if (!benhNhanHopLe)
            {
                ModelState.AddModelError(
                    "Form.MaBenhNhan",
                    "Người dùng thuốc không hợp lệ.");
            }


            if (!ModelState.IsValid)
            {
                var model =
                    await TaoPageModel(
                        maKhachHang.Value);

                model.Form = form;

                return View(
                    "Index",
                    model);
            }


            var lich =
                new LichNhacThuoc
                {
                    MaBenhNhan =
                        form.MaBenhNhan!.Value,

                    TenThuoc =
                        form.TenThuoc.Trim(),

                    LieuDung =
                        string.IsNullOrWhiteSpace(
                            form.LieuDung)
                            ? null
                            : form.LieuDung.Trim(),

                    CachDung =
                        string.IsNullOrWhiteSpace(
                            form.CachDung)
                            ? null
                            : form.CachDung.Trim(),

                    ThoiGianUong =
                        form.ThoiGianUong!.Value,

                    NgayBatDau =
                        form.NgayBatDau!.Value.Date,

                    NgayKetThuc =
                        form.NgayKetThuc?.Date,

                    TanSuat =
                        string.IsNullOrWhiteSpace(
                            form.TanSuat)
                            ? "Hằng ngày"
                            : form.TanSuat.Trim(),

                    GhiChu =
                        string.IsNullOrWhiteSpace(
                            form.GhiChu)
                            ? null
                            : form.GhiChu.Trim(),

                    TrangThai =
                        true,

                    NgayTao =
                        DateTime.Now
                };


            _context.LichNhacThuocs.Add(lich);

            await _context.SaveChangesAsync();


            TempData["Success"] =
                "Đã tạo lịch nhắc uống thuốc.";

            return RedirectToAction(
                nameof(Index));
        }


        // ============================================
        // BẬT / TẮT LỊCH
        // ============================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Toggle(
            int id)
        {
            int? maKhachHang =
                await LayMaKhachHangDangNhap();

            if (!maKhachHang.HasValue)
            {
                return Forbid();
            }


            var lich =
                await _context.LichNhacThuocs
                    .Include(x =>
                        x.BenhNhan)
                    .FirstOrDefaultAsync(x =>
                        x.MaLichNhac == id
                        &&
                        x.BenhNhan != null
                        &&
                        x.BenhNhan.MaKhachHang
                            == maKhachHang.Value);


            if (lich == null)
            {
                return NotFound();
            }


            lich.TrangThai =
                !lich.TrangThai;

            await _context.SaveChangesAsync();


            TempData["Success"] =
                lich.TrangThai
                    ? "Đã bật lịch nhắc."
                    : "Đã tạm dừng lịch nhắc.";

            return RedirectToAction(
                nameof(Index));
        }


        // ============================================
        // XÓA LỊCH
        // ============================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(
            int id)
        {
            int? maKhachHang =
                await LayMaKhachHangDangNhap();

            if (!maKhachHang.HasValue)
            {
                return Forbid();
            }


            var lich =
                await _context.LichNhacThuocs
                    .Include(x =>
                        x.BenhNhan)
                    .FirstOrDefaultAsync(x =>
                        x.MaLichNhac == id
                        &&
                        x.BenhNhan != null
                        &&
                        x.BenhNhan.MaKhachHang
                            == maKhachHang.Value);


            if (lich == null)
            {
                return NotFound();
            }


            _context.LichNhacThuocs
                .Remove(lich);

            await _context.SaveChangesAsync();


            TempData["Success"] =
                "Đã xóa lịch nhắc uống thuốc.";

            return RedirectToAction(
                nameof(Index));
        }


        // ============================================
        // NẠP DỮ LIỆU CHO TRANG
        // ============================================
        private async Task<
            CustomerMedicationReminderPageViewModel>
            TaoPageModel(
                int maKhachHang)
        {
            var nguoiThan =
                await _context.BenhNhans
                    .AsNoTracking()
                    .Where(x =>
                        x.MaKhachHang
                            == maKhachHang
                        &&
                        x.TrangThaiHoatDong)
                    .OrderBy(x =>
                        x.HoTen)
                    .Select(x =>
                        new CustomerMedicationRelativeOptionViewModel
                        {
                            MaBenhNhan =
                                x.MaBenhNhan,

                            HoTen =
                                x.HoTen,

                            QuanHe =
                                x.QuanHe
                                ?? "Người thân"
                        })
                    .ToListAsync();


            var lichNhac =
                await _context.LichNhacThuocs
                    .AsNoTracking()
                    .Where(x =>
                        x.BenhNhan != null
                        &&
                        x.BenhNhan.MaKhachHang
                            == maKhachHang
                        &&
                        x.BenhNhan.TrangThaiHoatDong)
                    .OrderByDescending(x =>
                        x.TrangThai)
                    .ThenBy(x =>
                        x.ThoiGianUong)
                    .Select(x =>
                        new CustomerMedicationReminderItemViewModel
                        {
                            MaLichNhac =
                                x.MaLichNhac,

                            TenBenhNhan =
                                x.BenhNhan!.HoTen,

                            TenThuoc =
                                x.TenThuoc,

                            LieuDung =
                                x.LieuDung,

                            CachDung =
                                x.CachDung,

                            ThoiGianUong =
                                x.ThoiGianUong,

                            NgayBatDau =
                                x.NgayBatDau,

                            NgayKetThuc =
                                x.NgayKetThuc,

                            TanSuat =
                                x.TanSuat,

                            GhiChu =
                                x.GhiChu,

                            TrangThai =
                                x.TrangThai,

                            LanNhacGanNhat =
                                x.LanNhacGanNhat
                        })
                    .ToListAsync();


            return
                new CustomerMedicationReminderPageViewModel
                {
                    NguoiThan =
                        nguoiThan,

                    LichNhac =
                        lichNhac
                };
        }
    }
}