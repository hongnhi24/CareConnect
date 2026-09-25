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
    public class ProfileSetupController : Controller
    {
        private readonly CareConnectDbContext _context;

        public ProfileSetupController(
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

            var nguoiChamSoc =
                await _context.NguoiChamSocs
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x =>
                        x.MaTaiKhoan == maTaiKhoan);

            if (nguoiChamSoc == null)
            {
                return NotFound(
                    "Không tìm thấy hồ sơ người chăm sóc.");
            }

            if (nguoiChamSoc.TrangThai == "Đã duyệt")
            {
                return RedirectToAction(
                    "Index",
                    "Dashboard",
                    new { area = "Caregiver" });
            }

            if (nguoiChamSoc.TrangThai == "Chờ duyệt")
            {
                return RedirectToAction(
                    nameof(Pending));
            }

            var model =
                new CaregiverProfileSetupViewModel
                {
                    HoTen =
                        nguoiChamSoc.HoTen
                        ?? string.Empty,

                    SoDienThoai =
                        nguoiChamSoc.SoDienThoai
                        ?? string.Empty,

                    ChuyenMon =
                        string.IsNullOrWhiteSpace(
                            nguoiChamSoc.ChuyenMon)
                            ? new List<string>()
                            : nguoiChamSoc.ChuyenMon
                                .Split(
                                    ',',
                                    StringSplitOptions
                                        .RemoveEmptyEntries)
                                .Select(x => x.Trim())
                                .ToList(),

                    KinhNghiem =
                        nguoiChamSoc.KinhNghiem
                        ?? 0,

                    KhuVucHoatDong =
                        nguoiChamSoc.KhuVucHoatDong
                        ?? string.Empty,

                    GiaTheoGio =
                        nguoiChamSoc.GiaTheoGio
                        ?? 0,

                    GioiThieu =
                        nguoiChamSoc.GioiThieu
                        ?? string.Empty
                };

            return RedirectToAction(
        "Index",
        "Profile",
        new { area = "Caregiver" });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(
            CaregiverProfileSetupViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

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

            if (nguoiChamSoc.TrangThai == "Đã duyệt")
            {
                return RedirectToAction(
                    "Index",
                    "Dashboard",
                    new { area = "Caregiver" });
            }

            nguoiChamSoc.HoTen =
                model.HoTen.Trim();

            nguoiChamSoc.SoDienThoai =
                model.SoDienThoai.Trim();

            nguoiChamSoc.ChuyenMon =
                string.Join(
                    ", ",
                    model.ChuyenMon
                        .Where(x =>
                            !string.IsNullOrWhiteSpace(x))
                        .Select(x => x.Trim())
                        .Distinct());

            nguoiChamSoc.KinhNghiem =
                model.KinhNghiem;

            nguoiChamSoc.KhuVucHoatDong =
                model.KhuVucHoatDong.Trim();

            nguoiChamSoc.GiaTheoGio =
                model.GiaTheoGio;

            nguoiChamSoc.GioiThieu =
                model.GioiThieu.Trim();

            // Sau khi gửi, Admin phải duyệt.
            nguoiChamSoc.TrangThai =
                "Chờ duyệt";

            await _context.SaveChangesAsync();

            return RedirectToAction(
                nameof(Pending));
        }

        [HttpGet]
        public async Task<IActionResult> Pending()
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

            var trangThai =
                await _context.NguoiChamSocs
                    .AsNoTracking()
                    .Where(x =>
                        x.MaTaiKhoan == maTaiKhoan)
                    .Select(x => x.TrangThai)
                    .FirstOrDefaultAsync();

            if (trangThai == "Đã duyệt")
            {
                return RedirectToAction(
                    "Index",
                    "Dashboard",
                    new { area = "Caregiver" });
            }

            return View();
        }
    }
}