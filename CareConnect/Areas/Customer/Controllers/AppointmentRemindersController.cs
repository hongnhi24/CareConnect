using System.Security.Claims;
using CareConnect.Data;
using CareConnect.Models;
using CareConnect.Services;
using CareConnect.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CareConnect.Areas.Customer.Controllers
{
    [Area("Customer")]
    [Authorize(Roles = "Khách hàng")]
    public class AppointmentRemindersController : Controller
    {
        private readonly CareConnectDbContext _context;

        public AppointmentRemindersController(CareConnectDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            int? maKhachHang = await LayMaKhachHangDangNhap();

            if (!maKhachHang.HasValue)
            {
                return Forbid();
            }

            var model = await TaoPageModel(maKhachHang.Value);
            DateTime now = VietnamClock.Now;

            model.Form.NgayTaiKham = now.Date.AddDays(1);
            model.Form.GioTaiKham = new TimeSpan(8, 0, 0);
            model.Form.SoPhutNhacTruoc = 1440;

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            [Bind(Prefix = "Form")]
            CustomerAppointmentReminderFormViewModel form)
        {
            int? maKhachHang = await LayMaKhachHangDangNhap();

            if (!maKhachHang.HasValue)
            {
                return Forbid();
            }

            bool benhNhanHopLe =
                form.MaBenhNhan.HasValue
                && await _context.BenhNhans
                    .AsNoTracking()
                    .AnyAsync(x =>
                        x.MaBenhNhan == form.MaBenhNhan.Value
                        && x.MaKhachHang == maKhachHang.Value
                        && x.TrangThaiHoatDong);

            if (!benhNhanHopLe)
            {
                ModelState.AddModelError(
                    "Form.MaBenhNhan",
                    "Người cần tái khám không hợp lệ.");
            }

            if (form.NgayTaiKham.HasValue && form.GioTaiKham.HasValue)
            {
                DateTime thoiDiemTaiKham =
                    form.NgayTaiKham.Value.Date.Add(form.GioTaiKham.Value);

                if (thoiDiemTaiKham <= VietnamClock.Now)
                {
                    ModelState.AddModelError(
                        "Form.NgayTaiKham",
                        "Thời điểm tái khám phải ở trong tương lai.");
                }
            }

            if (!ModelState.IsValid)
            {
                var model = await TaoPageModel(maKhachHang.Value);
                model.Form = form;
                return View("Index", model);
            }

            var lich = new LichNhacTaiKham
            {
                MaBenhNhan = form.MaBenhNhan!.Value,
                NgayTaiKham = form.NgayTaiKham!.Value.Date,
                GioTaiKham = form.GioTaiKham!.Value,
                NoiDung = form.NoiDung.Trim(),
                DiaDiem = string.IsNullOrWhiteSpace(form.DiaDiem)
                    ? null
                    : form.DiaDiem.Trim(),
                GhiChu = string.IsNullOrWhiteSpace(form.GhiChu)
                    ? null
                    : form.GhiChu.Trim(),
                SoPhutNhacTruoc = form.SoPhutNhacTruoc,
                TrangThai = true,
                NgayTao = VietnamClock.Now,
                LanNhacGanNhat = null
            };

            _context.LichNhacTaiKhams.Add(lich);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Đã tạo lịch nhắc tái khám.";

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Toggle(int id)
        {
            int? maKhachHang = await LayMaKhachHangDangNhap();

            if (!maKhachHang.HasValue)
            {
                return Forbid();
            }

            var lich = await _context.LichNhacTaiKhams
                .Include(x => x.BenhNhan)
                .FirstOrDefaultAsync(x =>
                    x.MaLichNhacTaiKham == id
                    && x.BenhNhan != null
                    && x.BenhNhan.MaKhachHang == maKhachHang.Value);

            if (lich == null)
            {
                return NotFound();
            }

            lich.TrangThai = !lich.TrangThai;

            if (lich.TrangThai)
            {
                lich.LanNhacGanNhat = null;
            }

            await _context.SaveChangesAsync();

            TempData["Success"] = lich.TrangThai
                ? "Đã bật lịch nhắc tái khám."
                : "Đã tạm dừng lịch nhắc tái khám.";

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            int? maKhachHang = await LayMaKhachHangDangNhap();

            if (!maKhachHang.HasValue)
            {
                return Forbid();
            }

            var lich = await _context.LichNhacTaiKhams
                .Include(x => x.BenhNhan)
                .FirstOrDefaultAsync(x =>
                    x.MaLichNhacTaiKham == id
                    && x.BenhNhan != null
                    && x.BenhNhan.MaKhachHang == maKhachHang.Value);

            if (lich == null)
            {
                return NotFound();
            }

            _context.LichNhacTaiKhams.Remove(lich);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Đã xóa lịch nhắc tái khám.";

            return RedirectToAction(nameof(Index));
        }

        private async Task<int?> LayMaKhachHangDangNhap()
        {
            string? maTaiKhoanClaim =
                User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (!int.TryParse(maTaiKhoanClaim, out int maTaiKhoan))
            {
                return null;
            }

            return await _context.KhachHangs
                .AsNoTracking()
                .Where(x =>
                    x.MaTaiKhoan == maTaiKhoan
                    && x.TrangThaiHoatDong)
                .Select(x => (int?)x.MaKhachHang)
                .FirstOrDefaultAsync();
        }

        private async Task<CustomerAppointmentReminderPageViewModel>
            TaoPageModel(int maKhachHang)
        {
            var nguoiThan = await _context.BenhNhans
                .AsNoTracking()
                .Where(x =>
                    x.MaKhachHang == maKhachHang
                    && x.TrangThaiHoatDong)
                .OrderBy(x => x.HoTen)
                .Select(x => new CustomerAppointmentRelativeOptionViewModel
                {
                    MaBenhNhan = x.MaBenhNhan,
                    HoTen = x.HoTen,
                    QuanHe = x.QuanHe ?? "Người thân"
                })
                .ToListAsync();

            var lichNhac = await _context.LichNhacTaiKhams
                .AsNoTracking()
                .Where(x =>
                    x.BenhNhan != null
                    && x.BenhNhan.MaKhachHang == maKhachHang
                    && x.BenhNhan.TrangThaiHoatDong)
                .OrderByDescending(x => x.TrangThai)
                .ThenBy(x => x.NgayTaiKham)
                .ThenBy(x => x.GioTaiKham)
                .Select(x => new CustomerAppointmentReminderItemViewModel
                {
                    MaLichNhacTaiKham = x.MaLichNhacTaiKham,
                    TenBenhNhan = x.BenhNhan!.HoTen,
                    NgayTaiKham = x.NgayTaiKham,
                    GioTaiKham = x.GioTaiKham,
                    NoiDung = x.NoiDung,
                    DiaDiem = x.DiaDiem,
                    GhiChu = x.GhiChu,
                    SoPhutNhacTruoc = x.SoPhutNhacTruoc,
                    TrangThai = x.TrangThai,
                    LanNhacGanNhat = x.LanNhacGanNhat
                })
                .ToListAsync();

            return new CustomerAppointmentReminderPageViewModel
            {
                NguoiThan = nguoiThan,
                LichNhac = lichNhac
            };
        }
    }
}
