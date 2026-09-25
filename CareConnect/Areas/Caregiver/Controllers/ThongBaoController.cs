using System.Security.Claims;
using CareConnect.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CareConnect.Areas.Caregiver.Controllers
{
    [Area("Caregiver")]
    [Authorize(Roles = "Người chăm sóc")]
    public class ThongBaoController : Controller
    {
        private readonly CareConnectDbContext _context;

        public ThongBaoController(
            CareConnectDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MoThongBao(
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

            /*
             * Hiện bảng ThongBao chưa có đường dẫn đích.
             * Tạm chuyển về trang Quản lý lịch vì phần lớn
             * thông báo của người chăm sóc liên quan đến lịch.
             */
            return RedirectToAction(
                "Index",
                "LichLamViec",
                new { area = "Caregiver" });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult>
            DanhDauTatCaDaDoc()
        {
            int? maTaiKhoan =
                LayMaTaiKhoan();

            if (!maTaiKhoan.HasValue)
            {
                return Unauthorized();
            }

            var danhSachChuaDoc =
                await _context.ThongBaos
                    .Where(x =>
                        x.MaTaiKhoan
                            == maTaiKhoan.Value
                        &&
                        !x.DaDoc)
                    .ToListAsync();

            if (danhSachChuaDoc.Count > 0)
            {
                foreach (var thongBao in
                    danhSachChuaDoc)
                {
                    thongBao.DaDoc = true;
                }

                await _context.SaveChangesAsync();
            }

            return Redirect(
                Request.Headers.Referer.ToString()
                is { Length: > 0 } referer
                    ? referer
                    : Url.Action(
                        "Index",
                        "Dashboard",
                        new { area = "Caregiver" })
                      ?? "/Caregiver/Dashboard");
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