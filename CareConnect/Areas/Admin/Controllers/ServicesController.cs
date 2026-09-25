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
    public class ServicesController : Controller
    {
        private readonly CareConnectDbContext _context;

        public ServicesController(
            CareConnectDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Index(
            string? tuKhoa,
            string? trangThai)
        {
            tuKhoa = tuKhoa?.Trim() ?? string.Empty;
            trangThai = trangThai?.Trim() ?? string.Empty;

            var query = _context.DichVus
                .AsNoTracking()
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(tuKhoa))
            {
                query = query.Where(x =>
                    x.TenDichVu.Contains(tuKhoa) ||
                    x.MaDichVuCode.Contains(tuKhoa) ||
                    (x.MoTa != null &&
                     x.MoTa.Contains(tuKhoa)));
            }

            if (trangThai == "Hoạt động")
            {
                query = query.Where(x => x.TrangThai);
            }
            else if (trangThai == "Tạm ngừng")
            {
                query = query.Where(x => !x.TrangThai);
            }

            var danhSachDichVu = await query
                .OrderByDescending(x => x.TrangThai)
                .ThenBy(x => x.MaDichVu)
                .Select(x => new AdminServiceItemViewModel
                {
                    MaDichVu = x.MaDichVu,

                    MaDichVuCode =
                        x.MaDichVuCode,

                    TenDichVu =
                        x.TenDichVu,

                    MoTa =
                        x.MoTa ?? "Chưa có mô tả",

                    Gia =
                        x.Gia,

                    ThoiLuong =
                        x.ThoiLuong,

                    TrangThai =
                        x.TrangThai,

                    NgayTao =
                        x.NgayTao
                })
                .ToListAsync();

            int tongDichVu = await _context.DichVus
                .AsNoTracking()
                .CountAsync();

            int dangHoatDong = await _context.DichVus
                .AsNoTracking()
                .CountAsync(x => x.TrangThai);

            int tamNgung = await _context.DichVus
                .AsNoTracking()
                .CountAsync(x => !x.TrangThai);

            decimal giaTrungBinh = tongDichVu == 0
                ? 0
                : await _context.DichVus
                    .AsNoTracking()
                    .AverageAsync(x => x.Gia);

            var model = new AdminServiceViewModel
            {
                TongDichVu =
                    tongDichVu,

                DangHoatDong =
                    dangHoatDong,

                TamNgung =
                    tamNgung,

                GiaTrungBinh =
                    giaTrungBinh,

                TuKhoa =
                    tuKhoa,

                TrangThai =
                    trangThai,

                DanhSachDichVu =
                    danhSachDichVu
            };

            return View(model);
        }

        [HttpGet]
        public IActionResult Create()
        {
            var model = new AdminServiceFormViewModel
            {
                TrangThai = true
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            AdminServiceFormViewModel model)
        {
            model.TenDichVu =
                model.TenDichVu?.Trim() ?? string.Empty;

            model.MaDichVuCode =
                model.MaDichVuCode?.Trim().ToUpper()
                ?? string.Empty;

            model.MoTa =
                model.MoTa?.Trim();

            bool trungMa = await _context.DichVus
                .AnyAsync(x =>
                    x.MaDichVuCode == model.MaDichVuCode);

            if (trungMa)
            {
                ModelState.AddModelError(
                    nameof(model.MaDichVuCode),
                    "Mã dịch vụ đã tồn tại.");
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var dichVu = new DichVu
            {
                TenDichVu =
                    model.TenDichVu,

                MoTa =
                    model.MoTa,

                Gia =
                    model.Gia,

                ThoiLuong =
                    model.ThoiLuong,

                TrangThai =
                    model.TrangThai,

                NgayTao =
                    DateTime.Now,

                MaDichVuCode =
                    model.MaDichVuCode
            };

            _context.DichVus.Add(dichVu);

            await _context.SaveChangesAsync();

            TempData["Success"] =
                $"Đã thêm dịch vụ “{dichVu.TenDichVu}”.";

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Edit(
            int id)
        {
            var dichVu = await _context.DichVus
                .AsNoTracking()
                .FirstOrDefaultAsync(x =>
                    x.MaDichVu == id);

            if (dichVu == null)
            {
                return NotFound();
            }

            var model = new AdminServiceFormViewModel
            {
                MaDichVu =
                    dichVu.MaDichVu,

                TenDichVu =
                    dichVu.TenDichVu,

                MoTa =
                    dichVu.MoTa,

                Gia =
                    dichVu.Gia,

                ThoiLuong =
                    dichVu.ThoiLuong,

                TrangThai =
                    dichVu.TrangThai,

                MaDichVuCode =
                    dichVu.MaDichVuCode
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            AdminServiceFormViewModel model)
        {
            if (id != model.MaDichVu)
            {
                return BadRequest();
            }

            model.TenDichVu =
                model.TenDichVu?.Trim() ?? string.Empty;

            model.MaDichVuCode =
                model.MaDichVuCode?.Trim().ToUpper()
                ?? string.Empty;

            model.MoTa =
                model.MoTa?.Trim();

            bool trungMa = await _context.DichVus
                .AnyAsync(x =>
                    x.MaDichVuCode == model.MaDichVuCode &&
                    x.MaDichVu != model.MaDichVu);

            if (trungMa)
            {
                ModelState.AddModelError(
                    nameof(model.MaDichVuCode),
                    "Mã dịch vụ đã được sử dụng.");
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var dichVu = await _context.DichVus
                .FirstOrDefaultAsync(x =>
                    x.MaDichVu == model.MaDichVu);

            if (dichVu == null)
            {
                return NotFound();
            }

            dichVu.TenDichVu =
                model.TenDichVu;

            dichVu.MoTa =
                model.MoTa;

            dichVu.Gia =
                model.Gia;

            dichVu.ThoiLuong =
                model.ThoiLuong;

            dichVu.TrangThai =
                model.TrangThai;

            dichVu.MaDichVuCode =
                model.MaDichVuCode;

            await _context.SaveChangesAsync();

            TempData["Success"] =
                $"Đã cập nhật dịch vụ “{dichVu.TenDichVu}”.";

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleStatus(
            int id)
        {
            var dichVu = await _context.DichVus
                .FirstOrDefaultAsync(x =>
                    x.MaDichVu == id);

            if (dichVu == null)
            {
                TempData["Error"] =
                    "Không tìm thấy dịch vụ.";

                return RedirectToAction(nameof(Index));
            }

            dichVu.TrangThai =
                !dichVu.TrangThai;

            await _context.SaveChangesAsync();

            TempData["Success"] =
                dichVu.TrangThai
                    ? $"Đã kích hoạt dịch vụ “{dichVu.TenDichVu}”."
                    : $"Đã tạm ngừng dịch vụ “{dichVu.TenDichVu}”.";

            return RedirectToAction(nameof(Index));
        }
    }
}