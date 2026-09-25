using CareConnect.Data;
using CareConnect.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace CareConnect.Areas.Customer.Controllers
{
    [Area("Customer")]
    [Authorize]
    public class NotificationsController : Controller
    {
        private readonly CareConnectDbContext _context;

        public NotificationsController(
            CareConnectDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Open(
    int maThongBao)
        {
            int? maTaiKhoan =
                LayMaTaiKhoan();

            if (!maTaiKhoan.HasValue)
            {
                return Unauthorized();
            }

            var thongBao =
                await _context.ThongBaos
                    .FirstOrDefaultAsync(x =>
                        x.MaThongBao == maThongBao
                        &&
                        x.MaTaiKhoan
                            == maTaiKhoan.Value);

            if (thongBao != null &&
                !thongBao.DaDoc)
            {
                thongBao.DaDoc = true;

                await _context.SaveChangesAsync();
            }

            if (thongBao != null &&
                thongBao.TieuDe == "Nhắc uống thuốc")
            {
                return RedirectToAction(
                    "Index",
                    "MedicationReminders",
                    new
                    {
                        area = "Customer"
                    });
            }

            return RedirectToAction(
                "Index",
                "Dashboard",
                new
                {
                    area = "Customer"
                });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult>
            MarkAllAsRead()
        {
            int? maTaiKhoan =
                LayMaTaiKhoan();

            if (!maTaiKhoan.HasValue)
            {
                return Unauthorized();
            }

            var danhSachThongBao =
                await _context.ThongBaos
                    .Where(x =>
                        x.MaTaiKhoan
                            == maTaiKhoan.Value
                        &&
                        !x.DaDoc)
                    .ToListAsync();

            foreach (var thongBao in
                danhSachThongBao)
            {
                thongBao.DaDoc = true;
            }

            if (danhSachThongBao.Count > 0)
            {
                await _context.SaveChangesAsync();
            }

            string? referer =
                Request.Headers.Referer.ToString();

            if (!string.IsNullOrWhiteSpace(referer))
            {
                return Redirect(referer);
            }
            
            return RedirectToAction(
                "Index",
                "Dashboard",
                new { area = "Customer" });
        }

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