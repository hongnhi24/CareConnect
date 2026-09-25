using CareConnect.Data;
using CareConnect.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CareConnect.ViewComponents
{
    public class AdminSidebarViewComponent
        : ViewComponent
    {
        private readonly CareConnectDbContext _context;

        public AdminSidebarViewComponent(
            CareConnectDbContext context)
        {
            _context = context;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            int soHoSoChoDuyet =
                await _context.NguoiChamSocs
                    .AsNoTracking()
                    .CountAsync(x =>
                        x.TrangThai == "Chờ duyệt");

            int soTaiKhoanBiKhoa =
                await _context.TaiKhoans
                    .AsNoTracking()
                    .CountAsync(x =>
                        !x.TrangThai);

            int soLichChoXacNhan =
                await _context.DatLichs
                    .AsNoTracking()
                    .CountAsync(x =>
                        x.TrangThai == "Chờ xác nhận");

            string controllerHienTai =
                ViewContext.RouteData.Values["controller"]?
                    .ToString()
                ?? string.Empty;

            var model = new AdminSidebarViewModel
            {
                SoHoSoChoDuyet =
                    soHoSoChoDuyet,

                SoTaiKhoanBiKhoa =
                    soTaiKhoanBiKhoa,

                SoLichChoXacNhan =
                    soLichChoXacNhan,

                ControllerHienTai =
                    controllerHienTai
            };

            return View(model);
        }
    }
}