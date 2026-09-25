using CareConnect.Constants;
using CareConnect.Data;
using CareConnect.Models;
using CareConnect.Services;
using CareConnect.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace CareConnect.Areas.Caregiver.Controllers
{
    [Area("Caregiver")]
    [Authorize(Roles = "Người chăm sóc")]
    public class LichKhaDungController : Controller
    {
        private const string TrangThaiKhaDung =
            "Khả dụng";

        private const string TrangThaiTamNghi =
            "Tạm nghỉ";

        private readonly CareConnectDbContext _context;

        public LichKhaDungController(
            CareConnectDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Index(
            DateTime? tuNgay)
        {
            NguoiChamSoc? nguoiChamSoc =
                await LayNguoiChamSocAsync();

            if (nguoiChamSoc == null)
            {
                return RedirectToAction(
                    "Login",
                    "Account",
                    new { area = "" });
            }

            DateTime homNay =
                VietnamClock.Now.Date;

            DateTime ngayThamChieu =
                (tuNgay ?? homNay).Date;

            DateTime dauTuan =
                LayDauTuan(ngayThamChieu);

            DateTime dauTuanSau =
                dauTuan.AddDays(7);

            var lichTrongTuan =
                await _context.LichLamViecs
                    .AsNoTracking()
                    .Where(x =>
                        x.MaNguoiChamSoc
                            == nguoiChamSoc.MaNguoiChamSoc
                        && x.NgayLam >= dauTuan
                        && x.NgayLam < dauTuanSau)
                    .OrderBy(x => x.NgayLam)
                    .ThenBy(x => x.GioBatDau)
                    .ToListAsync();

            var datLichTrongTuan =
                await _context.DatLichs
                    .AsNoTracking()
                    .Where(x =>
                        x.MaNguoiChamSoc
                            == nguoiChamSoc.MaNguoiChamSoc
                        && x.NgayChamSoc.HasValue
                        && x.NgayChamSoc.Value >= dauTuan
                        && x.NgayChamSoc.Value < dauTuanSau
                        && x.TrangThai
                            != BookingStatus.DaHuy
                        && x.GioBatDau.HasValue
                        && x.GioKetThuc.HasValue)
                    .Select(x => new
                    {
                        Ngay = x.NgayChamSoc!.Value,
                        GioBatDau = x.GioBatDau!.Value,
                        GioKetThuc = x.GioKetThuc!.Value
                    })
                    .ToListAsync();

            var model =
                new CaregiverAvailabilityViewModel
                {
                    DauTuan = dauTuan,
                    CuoiTuan =
                        dauTuan.AddDays(6),
                    HomNay = homNay
                };

            for (int i = 0; i < 7; i++)
            {
                DateTime ngay =
                    dauTuan.AddDays(i).Date;

                var day =
                    new CaregiverAvailabilityDayViewModel
                    {
                        Ngay = ngay,
                        Thu = LayTenThu(ngay),
                        LaHomNay =
                            ngay == homNay
                    };

                foreach (LichLamViec lich
                         in lichTrongTuan.Where(x =>
                             x.NgayLam.Date == ngay))
                {
                    int soLichDat =
                        datLichTrongTuan.Count(x =>
                            x.Ngay.Date == ngay
                            && x.GioBatDau
                                < lich.GioKetThuc
                            && x.GioKetThuc
                                > lich.GioBatDau);

                    day.DanhSachKhungGio.Add(
                        new CaregiverAvailabilityItemViewModel
                        {
                            MaLich = lich.MaLich,
                            NgayLam = lich.NgayLam,
                            GioBatDau = lich.GioBatDau,
                            GioKetThuc = lich.GioKetThuc,
                            TrangThai =
                                string.IsNullOrWhiteSpace(
                                    lich.TrangThai)
                                    ? TrangThaiKhaDung
                                    : lich.TrangThai!,
                            CoLichDat = soLichDat > 0,
                            SoLichDat = soLichDat
                        });
                }

                model.DanhSachNgay.Add(day);
            }

            var lichKhaDung =
                lichTrongTuan.Where(
                    LaKhungKhaDung);

            model.TongKhungKhaDung =
                lichKhaDung.Count();

            model.TongGioKhaDung =
                Math.Round(
                    lichKhaDung.Sum(x =>
                        Math.Max(
                            0,
                            (x.GioKetThuc
                                - x.GioBatDau)
                            .TotalHours)),
                    1);

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Them(
            CaregiverAvailabilityCreateViewModel model,
            DateTime? tuNgay)
        {
            NguoiChamSoc? nguoiChamSoc =
                await LayNguoiChamSocAsync();

            if (nguoiChamSoc == null)
            {
                return RedirectToAction(
                    "Login",
                    "Account",
                    new { area = "" });
            }

            string? loi =
                await KiemTraKhungGioAsync(
                    nguoiChamSoc.MaNguoiChamSoc,
                    model.NgayLam,
                    model.GioBatDau,
                    model.GioKetThuc,
                    null);

            if (!string.IsNullOrWhiteSpace(loi))
            {
                TempData["Error"] = loi;

                return RedirectVeTuan(
                    tuNgay ?? model.NgayLam);
            }

            var lich =
                new LichLamViec
                {
                    MaNguoiChamSoc =
                        nguoiChamSoc.MaNguoiChamSoc,
                    NgayLam = model.NgayLam.Date,
                    GioBatDau = model.GioBatDau,
                    GioKetThuc = model.GioKetThuc,
                    TrangThai = TrangThaiKhaDung
                };

            _context.LichLamViecs.Add(lich);
            await _context.SaveChangesAsync();

            TempData["Success"] =
                "Đã thêm khung giờ khả dụng.";

            return RedirectVeTuan(
                tuNgay ?? model.NgayLam);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CapNhat(
            CaregiverAvailabilityUpdateViewModel model,
            DateTime? tuNgay)
        {
            NguoiChamSoc? nguoiChamSoc =
                await LayNguoiChamSocAsync();

            if (nguoiChamSoc == null)
            {
                return RedirectToAction(
                    "Login",
                    "Account",
                    new { area = "" });
            }

            LichLamViec? lich =
                await _context.LichLamViecs
                    .FirstOrDefaultAsync(x =>
                        x.MaLich == model.MaLich
                        && x.MaNguoiChamSoc
                            == nguoiChamSoc.MaNguoiChamSoc);

            if (lich == null)
            {
                TempData["Error"] =
                    "Không tìm thấy khung giờ cần cập nhật.";

                return RedirectVeTuan(
                    tuNgay ?? VietnamClock.Now.Date);
            }

            if (lich.NgayLam.Date
                < VietnamClock.Now.Date)
            {
                TempData["Error"] =
                    "Không thể chỉnh sửa lịch khả dụng đã qua.";

                return RedirectVeTuan(
                    tuNgay ?? lich.NgayLam);
            }

            if (await CoDatLichGiaoNhauAsync(
                    nguoiChamSoc.MaNguoiChamSoc,
                    lich.NgayLam,
                    lich.GioBatDau,
                    lich.GioKetThuc))
            {
                TempData["Error"] =
                    "Khung giờ này đã có lịch chăm sóc. "
                    + "Bạn không thể thay đổi thời gian của khung này.";

                return RedirectVeTuan(
                    tuNgay ?? lich.NgayLam);
            }

            string? loi =
                await KiemTraKhungGioAsync(
                    nguoiChamSoc.MaNguoiChamSoc,
                    model.NgayLam,
                    model.GioBatDau,
                    model.GioKetThuc,
                    model.MaLich);

            if (!string.IsNullOrWhiteSpace(loi))
            {
                TempData["Error"] = loi;

                return RedirectVeTuan(
                    tuNgay ?? model.NgayLam);
            }

            lich.NgayLam = model.NgayLam.Date;
            lich.GioBatDau = model.GioBatDau;
            lich.GioKetThuc = model.GioKetThuc;

            await _context.SaveChangesAsync();

            TempData["Success"] =
                "Đã cập nhật khung giờ khả dụng.";

            return RedirectVeTuan(
                tuNgay ?? model.NgayLam);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DoiTrangThai(
            int maLich,
            DateTime? tuNgay)
        {
            NguoiChamSoc? nguoiChamSoc =
                await LayNguoiChamSocAsync();

            if (nguoiChamSoc == null)
            {
                return RedirectToAction(
                    "Login",
                    "Account",
                    new { area = "" });
            }

            LichLamViec? lich =
                await _context.LichLamViecs
                    .FirstOrDefaultAsync(x =>
                        x.MaLich == maLich
                        && x.MaNguoiChamSoc
                            == nguoiChamSoc.MaNguoiChamSoc);

            if (lich == null)
            {
                TempData["Error"] =
                    "Không tìm thấy khung giờ.";

                return RedirectVeTuan(
                    tuNgay ?? VietnamClock.Now.Date);
            }

            if (lich.NgayLam.Date
                < VietnamClock.Now.Date)
            {
                TempData["Error"] =
                    "Không thể thay đổi lịch khả dụng đã qua.";

                return RedirectVeTuan(
                    tuNgay ?? lich.NgayLam);
            }

            bool dangKhaDung =
                LaKhungKhaDung(lich);

            if (dangKhaDung
                && await CoDatLichGiaoNhauAsync(
                    nguoiChamSoc.MaNguoiChamSoc,
                    lich.NgayLam,
                    lich.GioBatDau,
                    lich.GioKetThuc))
            {
                TempData["Error"] =
                    "Khung giờ này đã có lịch chăm sóc nên "
                    + "không thể chuyển sang tạm nghỉ.";

                return RedirectVeTuan(
                    tuNgay ?? lich.NgayLam);
            }

            lich.TrangThai =
                dangKhaDung
                    ? TrangThaiTamNghi
                    : TrangThaiKhaDung;

            await _context.SaveChangesAsync();

            TempData["Success"] =
                dangKhaDung
                    ? "Đã chuyển khung giờ sang tạm nghỉ."
                    : "Đã mở lại khung giờ khả dụng.";

            return RedirectVeTuan(
                tuNgay ?? lich.NgayLam);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Xoa(
            int maLich,
            DateTime? tuNgay)
        {
            NguoiChamSoc? nguoiChamSoc =
                await LayNguoiChamSocAsync();

            if (nguoiChamSoc == null)
            {
                return RedirectToAction(
                    "Login",
                    "Account",
                    new { area = "" });
            }

            LichLamViec? lich =
                await _context.LichLamViecs
                    .FirstOrDefaultAsync(x =>
                        x.MaLich == maLich
                        && x.MaNguoiChamSoc
                            == nguoiChamSoc.MaNguoiChamSoc);

            if (lich == null)
            {
                TempData["Error"] =
                    "Không tìm thấy khung giờ cần xóa.";

                return RedirectVeTuan(
                    tuNgay ?? VietnamClock.Now.Date);
            }

            if (lich.NgayLam.Date
                < VietnamClock.Now.Date)
            {
                TempData["Error"] =
                    "Không xóa lịch khả dụng đã qua để giữ số liệu thống kê.";

                return RedirectVeTuan(
                    tuNgay ?? lich.NgayLam);
            }

            if (await CoDatLichGiaoNhauAsync(
                    nguoiChamSoc.MaNguoiChamSoc,
                    lich.NgayLam,
                    lich.GioBatDau,
                    lich.GioKetThuc))
            {
                TempData["Error"] =
                    "Khung giờ này đã có lịch chăm sóc nên không thể xóa.";

                return RedirectVeTuan(
                    tuNgay ?? lich.NgayLam);
            }

            _context.LichLamViecs.Remove(lich);
            await _context.SaveChangesAsync();

            TempData["Success"] =
                "Đã xóa khung giờ khả dụng.";

            return RedirectVeTuan(
                tuNgay ?? lich.NgayLam);
        }

        private async Task<string?>
            KiemTraKhungGioAsync(
                int maNguoiChamSoc,
                DateTime ngayLam,
                TimeSpan gioBatDau,
                TimeSpan gioKetThuc,
                int? maLichBoQua)
        {
            DateTime homNay =
                VietnamClock.Now.Date;

            ngayLam = ngayLam.Date;

            if (ngayLam < homNay)
            {
                return "Ngày khả dụng không được nhỏ hơn ngày hiện tại.";
            }

            if (gioKetThuc <= gioBatDau)
            {
                return "Giờ kết thúc phải lớn hơn giờ bắt đầu.";
            }

            var query =
                _context.LichLamViecs
                    .AsNoTracking()
                    .Where(x =>
                        x.MaNguoiChamSoc == maNguoiChamSoc
                        && x.NgayLam == ngayLam
                        && x.GioBatDau < gioKetThuc
                        && x.GioKetThuc > gioBatDau);

            if (maLichBoQua.HasValue)
            {
                int id = maLichBoQua.Value;

                query = query.Where(x =>
                    x.MaLich != id);
            }

            if (await query.AnyAsync())
            {
                return "Khung giờ mới đang trùng với một khung giờ đã tạo.";
            }

            return null;
        }

        private async Task<bool>
            CoDatLichGiaoNhauAsync(
                int maNguoiChamSoc,
                DateTime ngayLam,
                TimeSpan gioBatDau,
                TimeSpan gioKetThuc)
        {
            DateTime ngay = ngayLam.Date;
            DateTime ngayKeTiep = ngay.AddDays(1);

            return await _context.DatLichs
                .AsNoTracking()
                .AnyAsync(x =>
                    x.MaNguoiChamSoc == maNguoiChamSoc
                    && x.NgayChamSoc.HasValue
                    && x.NgayChamSoc.Value >= ngay
                    && x.NgayChamSoc.Value < ngayKeTiep
                    && x.GioBatDau.HasValue
                    && x.GioKetThuc.HasValue
                    && x.TrangThai != BookingStatus.DaHuy
                    && x.GioBatDau.Value < gioKetThuc
                    && x.GioKetThuc.Value > gioBatDau);
        }

        private async Task<NguoiChamSoc?>
            LayNguoiChamSocAsync()
        {
            int? maTaiKhoan = LayMaTaiKhoan();

            if (!maTaiKhoan.HasValue)
            {
                return null;
            }

            return await _context.NguoiChamSocs
                .FirstOrDefaultAsync(x =>
                    x.MaTaiKhoan
                        == maTaiKhoan.Value);
        }

        private int? LayMaTaiKhoan()
        {
            string? claimValue =
                User.FindFirstValue(
                    ClaimTypes.NameIdentifier);

            return int.TryParse(
                    claimValue,
                    out int maTaiKhoan)
                ? maTaiKhoan
                : null;
        }

        private IActionResult RedirectVeTuan(
            DateTime ngay)
        {
            DateTime dauTuan =
                LayDauTuan(ngay.Date);

            return RedirectToAction(
                nameof(Index),
                new
                {
                    tuNgay =
                        dauTuan.ToString(
                            "yyyy-MM-dd")
                });
        }

        private static DateTime LayDauTuan(
            DateTime ngay)
        {
            int khoangCach =
                ((int)ngay.DayOfWeek + 6) % 7;

            return ngay.Date.AddDays(
                -khoangCach);
        }

        private static string LayTenThu(
            DateTime ngay)
        {
            return ngay.DayOfWeek switch
            {
                DayOfWeek.Monday => "Thứ 2",
                DayOfWeek.Tuesday => "Thứ 3",
                DayOfWeek.Wednesday => "Thứ 4",
                DayOfWeek.Thursday => "Thứ 5",
                DayOfWeek.Friday => "Thứ 6",
                DayOfWeek.Saturday => "Thứ 7",
                _ => "Chủ nhật"
            };
        }

        private static bool LaKhungKhaDung(
            LichLamViec lich)
        {
            if (lich.GioKetThuc
                <= lich.GioBatDau)
            {
                return false;
            }

            string trangThai =
                lich.TrangThai?.Trim()
                ?? string.Empty;

            return !trangThai.Equals(
                       TrangThaiTamNghi,
                       StringComparison.OrdinalIgnoreCase)
                   && !trangThai.Equals(
                       "Không khả dụng",
                       StringComparison.OrdinalIgnoreCase)
                   && !trangThai.Equals(
                       "Nghỉ",
                       StringComparison.OrdinalIgnoreCase)
                   && !trangThai.Equals(
                       "Đã hủy",
                       StringComparison.OrdinalIgnoreCase);
        }
    }
}
