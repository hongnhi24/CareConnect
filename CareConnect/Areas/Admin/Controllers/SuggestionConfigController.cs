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
    public class SuggestionConfigController : Controller
    {
        private readonly CareConnectDbContext _context;

        public SuggestionConfigController(
            CareConnectDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var cauHinh =
                await _context.CauHinhGoiYs
                    .AsNoTracking()
                    .OrderByDescending(x => x.DangApDung)
                    .ThenByDescending(x => x.NgayCapNhat)
                    .FirstOrDefaultAsync();

            if (cauHinh == null)
            {
                var modelMacDinh =
                    new AdminSuggestionConfigViewModel
                    {
                        TenCauHinh =
                            "Cấu hình gợi ý mặc định",

                        TrongSoKhuVuc = 25,
                        TrongSoChuyenMon = 25,
                        TrongSoKinhNghiem = 15,
                        TrongSoDanhGia = 20,
                        TrongSoMucGia = 10,
                        TrongSoLichTrong = 5,
                        SoLuongGoiY = 5,
                        DiemDanhGiaToiThieu = 3,
                        KinhNghiemToiThieu = 0,
                        ChiGoiYNguoiDaDuyet = true,
                        UuTienCungKhuVuc = true,
                        DangApDung = true,
                        NgayCapNhat = DateTime.Now
                    };

                return View(modelMacDinh);
            }

            var model =
                new AdminSuggestionConfigViewModel
                {
                    MaCauHinh =
                        cauHinh.MaCauHinh,

                    TenCauHinh =
                        cauHinh.TenCauHinh,

                    TrongSoKhuVuc =
                        cauHinh.TrongSoKhuVuc,

                    TrongSoChuyenMon =
                        cauHinh.TrongSoChuyenMon,

                    TrongSoKinhNghiem =
                        cauHinh.TrongSoKinhNghiem,

                    TrongSoDanhGia =
                        cauHinh.TrongSoDanhGia,

                    TrongSoMucGia =
                        cauHinh.TrongSoMucGia,

                    TrongSoLichTrong =
                        cauHinh.TrongSoLichTrong,

                    SoLuongGoiY =
                        cauHinh.SoLuongGoiY,

                    DiemDanhGiaToiThieu =
                        cauHinh.DiemDanhGiaToiThieu,

                    KinhNghiemToiThieu =
                        cauHinh.KinhNghiemToiThieu,

                    ChiGoiYNguoiDaDuyet =
                        cauHinh.ChiGoiYNguoiDaDuyet,

                    UuTienCungKhuVuc =
                        cauHinh.UuTienCungKhuVuc,

                    DangApDung =
                        cauHinh.DangApDung,

                    NgayCapNhat =
                        cauHinh.NgayCapNhat,

                    NguoiCapNhat =
                        cauHinh.NguoiCapNhat
                };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Save(
            AdminSuggestionConfigViewModel model)
        {
            if (model.TongTrongSo != 100)
            {
                ModelState.AddModelError(
                    string.Empty,
                    $"Tổng trọng số hiện tại là {model.TongTrongSo}%. " +
                    "Tổng trọng số phải bằng 100%.");
            }

            if (!ModelState.IsValid)
            {
                return View(
                    "Index",
                    model);
            }

            CauHinhGoiY? cauHinh = null;

            if (model.MaCauHinh > 0)
            {
                cauHinh =
                    await _context.CauHinhGoiYs
                        .FirstOrDefaultAsync(x =>
                            x.MaCauHinh == model.MaCauHinh);
            }

            if (cauHinh == null)
            {
                cauHinh =
                    new CauHinhGoiY();

                _context.CauHinhGoiYs.Add(
                    cauHinh);
            }

            cauHinh.TenCauHinh =
                model.TenCauHinh.Trim();

            cauHinh.TrongSoKhuVuc =
                model.TrongSoKhuVuc;

            cauHinh.TrongSoChuyenMon =
                model.TrongSoChuyenMon;

            cauHinh.TrongSoKinhNghiem =
                model.TrongSoKinhNghiem;

            cauHinh.TrongSoDanhGia =
                model.TrongSoDanhGia;

            cauHinh.TrongSoMucGia =
                model.TrongSoMucGia;

            cauHinh.TrongSoLichTrong =
                model.TrongSoLichTrong;

            cauHinh.SoLuongGoiY =
                model.SoLuongGoiY;

            cauHinh.DiemDanhGiaToiThieu =
                model.DiemDanhGiaToiThieu;

            cauHinh.KinhNghiemToiThieu =
                model.KinhNghiemToiThieu;

            cauHinh.ChiGoiYNguoiDaDuyet =
                model.ChiGoiYNguoiDaDuyet;

            cauHinh.UuTienCungKhuVuc =
                model.UuTienCungKhuVuc;

            cauHinh.DangApDung =
                model.DangApDung;

            cauHinh.NgayCapNhat =
                DateTime.Now;

            cauHinh.NguoiCapNhat =
                User.Identity?.Name
                ?? "admin";

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] =
                "Đã lưu cấu hình gợi ý thành công.";

            return RedirectToAction(
                nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reset()
        {
            var cauHinh =
                await _context.CauHinhGoiYs
                    .OrderByDescending(x =>
                        x.DangApDung)
                    .FirstOrDefaultAsync();

            if (cauHinh == null)
            {
                cauHinh =
                    new CauHinhGoiY();

                _context.CauHinhGoiYs.Add(
                    cauHinh);
            }

            cauHinh.TenCauHinh =
                "Cấu hình gợi ý mặc định";

            cauHinh.TrongSoKhuVuc = 25;
            cauHinh.TrongSoChuyenMon = 25;
            cauHinh.TrongSoKinhNghiem = 15;
            cauHinh.TrongSoDanhGia = 20;
            cauHinh.TrongSoMucGia = 10;
            cauHinh.TrongSoLichTrong = 5;
            cauHinh.SoLuongGoiY = 5;
            cauHinh.DiemDanhGiaToiThieu = 3;
            cauHinh.KinhNghiemToiThieu = 0;
            cauHinh.ChiGoiYNguoiDaDuyet = true;
            cauHinh.UuTienCungKhuVuc = true;
            cauHinh.DangApDung = true;
            cauHinh.NgayCapNhat = DateTime.Now;
            cauHinh.NguoiCapNhat =
                User.Identity?.Name ?? "admin";

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] =
                "Đã khôi phục cấu hình mặc định.";

            return RedirectToAction(
                nameof(Index));
        }
    }
}