using System.Security.Claims;
using CareConnect.Data;
using CareConnect.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CareConnect.ViewComponents
{
    public class CaregiverHeaderViewComponent
        : ViewComponent
    {
        private readonly CareConnectDbContext _context;

        public CaregiverHeaderViewComponent(
            CareConnectDbContext context)
        {
            _context = context;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            int? maTaiKhoan =
                LayMaTaiKhoan();

            if (!maTaiKhoan.HasValue)
            {
                return View(
                    new CaregiverHeaderViewModel());
            }

            var nguoiChamSoc =
                await _context.NguoiChamSocs
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x =>
                        x.MaTaiKhoan
                            == maTaiKhoan.Value);

            string hoTen =
                nguoiChamSoc?.HoTen
                ?? User.Identity?.Name
                ?? "Người chăm sóc";

            string vietTat =
                TaoVietTat(hoTen);

            int soThongBaoChuaDoc =
                await _context.ThongBaos
                    .AsNoTracking()
                    .CountAsync(x =>
                        x.MaTaiKhoan
                            == maTaiKhoan.Value
                        &&
                        !x.DaDoc);

            var thongBaoGanDay =
                await _context.ThongBaos
                    .AsNoTracking()
                    .Where(x =>
                        x.MaTaiKhoan
                            == maTaiKhoan.Value)
                    .OrderByDescending(x =>
                        x.NgayGui)
                    .Take(6)
                    .Select(x =>
                        new
                        CaregiverHeaderNotificationViewModel
                        {
                            MaThongBao =
                                x.MaThongBao,

                            TieuDe =
                                x.TieuDe,

                            NoiDung =
                                x.NoiDung,

                            DaDoc =
                                x.DaDoc,

                            NgayGui =
                                x.NgayGui
                        })
                    .ToListAsync();

            var model =
                new CaregiverHeaderViewModel
                {
                    HoTen =
                        hoTen,

                    VietTat =
                        vietTat,

                    SoThongBaoChuaDoc =
                        soThongBaoChuaDoc,

                    ThongBaoGanDay =
                        thongBaoGanDay
                };

            return View(model);
        }

        private int? LayMaTaiKhoan()
        {
            string? claim =
                UserClaimsPrincipal
                    .FindFirstValue(
                        ClaimTypes.NameIdentifier);

            return int.TryParse(
                claim,
                out int maTaiKhoan)
                    ? maTaiKhoan
                    : null;
        }

        private static string TaoVietTat(
            string hoTen)
        {
            if (string.IsNullOrWhiteSpace(hoTen))
            {
                return "NCS";
            }

            return string.Join(
                    "",
                    hoTen
                        .Split(
                            ' ',
                            StringSplitOptions
                                .RemoveEmptyEntries)
                        .TakeLast(2)
                        .Select(x => x[0]))
                .ToUpper();
        }
    }
}