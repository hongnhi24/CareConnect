using CareConnect.Constants;
using CareConnect.Data;
using CareConnect.Models;
using CareConnect.Services;
using CareConnect.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text;

namespace CareConnect.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Quản trị viên")]
    public class DashboardController : Controller
    {
        private readonly CareConnectDbContext _context;

        public DashboardController(
            CareConnectDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            DateTime homNay = VietnamClock.Now.Date;

            DateTime dauThang =
                new DateTime(
                    homNay.Year,
                    homNay.Month,
                    1);

            DateTime dauThangSau =
                dauThang.AddMonths(1);

            int soNgayTinhTuThuHai =
                ((int)homNay.DayOfWeek + 6) % 7;

            DateTime dauTuan =
                homNay.AddDays(-soNgayTinhTuThuHai);

            DateTime dauTuanSau =
                dauTuan.AddDays(7);

            DateTime cuoiTuan =
                dauTuanSau.AddDays(-1);

            int tongTaiKhoan =
                await _context.TaiKhoans
                    .AsNoTracking()
                    .CountAsync();

            int tongKhachHang =
                await _context.KhachHangs
                    .AsNoTracking()
                    .CountAsync();

            int tongNguoiChamSoc =
                await _context.NguoiChamSocs
                    .AsNoTracking()
                    .CountAsync();

            int hoSoChoDuyet =
                await _context.NguoiChamSocs
                    .AsNoTracking()
                    .CountAsync(x =>
                        x.TrangThai == "Chờ duyệt");

            int lichChoXacNhan =
                await _context.DatLichs
                    .AsNoTracking()
                    .CountAsync(x =>
                        x.TrangThai
                            == BookingStatus.ChoXacNhan);

            int tongDatLichHomNay =
                await _context.DatLichs
                    .AsNoTracking()
                    .CountAsync(x =>
                        x.NgayChamSoc.HasValue
                        && x.NgayChamSoc.Value >= homNay
                        && x.NgayChamSoc.Value < homNay.AddDays(1)
                        && x.TrangThai != BookingStatus.DaHuy);

            int tongDatLichThang =
                await _context.DatLichs
                    .AsNoTracking()
                    .CountAsync(x =>
                        x.NgayChamSoc.HasValue
                        && x.NgayChamSoc.Value >= dauThang
                        && x.NgayChamSoc.Value < dauThangSau);

            decimal doanhThuThang =
                await _context.DatLichs
                    .AsNoTracking()
                    .Where(x =>
                        x.TrangThai
                            == BookingStatus.DaHoanThanh
                        && x.NgayChamSoc.HasValue
                        && x.NgayChamSoc.Value >= dauThang
                        && x.NgayChamSoc.Value < dauThangSau)
                    .SumAsync(x => x.TongTien ?? 0);

            var datLichTrongTuan =
                await _context.DatLichs
                    .AsNoTracking()
                    .Where(x =>
                        x.NgayChamSoc.HasValue
                        && x.NgayChamSoc.Value >= dauTuan
                        && x.NgayChamSoc.Value < dauTuanSau)
                    .ToListAsync();

            var lichLamViecTrongTuan =
                await _context.LichLamViecs
                    .AsNoTracking()
                    .Where(x =>
                        x.NgayLam >= dauTuan
                        && x.NgayLam < dauTuanSau)
                    .ToListAsync();

            var thongKeTuan =
                TaoThongKeTheoNgayTrongTuan(
                    dauTuan,
                    datLichTrongTuan,
                    lichLamViecTrongTuan);

            double tongGioMoLichTuan =
                Math.Round(
                    thongKeTuan.Sum(x => x.GioMoLich),
                    1);

            double tongGioDaDatTuan =
                Math.Round(
                    thongKeTuan.Sum(x => x.GioDaDat),
                    1);

            bool coDuLieuLichLamViec =
                tongGioMoLichTuan > 0;

            double tyLeLapDay =
                !coDuLieuLichLamViec
                    ? 0
                    : Math.Round(
                        Math.Min(
                            100,
                            tongGioDaDatTuan
                                * 100.0
                                / tongGioMoLichTuan),
                        1);

            int tongDatLichTuan =
                datLichTrongTuan.Count;

            int lichHoanThanhTuan =
                datLichTrongTuan.Count(x =>
                    x.TrangThai
                        == BookingStatus.DaHoanThanh);

            int lichDaHuyTuan =
                datLichTrongTuan.Count(x =>
                    x.TrangThai
                        == BookingStatus.DaHuy);

            var thongKeTheoThang =
                new List<AdminMonthlyStatisticViewModel>();

            for (int i = 5; i >= 0; i--)
            {
                DateTime batDau =
                    new DateTime(
                        homNay.Year,
                        homNay.Month,
                        1)
                    .AddMonths(-i);

                DateTime ketThuc =
                    batDau.AddMonths(1);

                int soDatLich =
                    await _context.DatLichs
                        .AsNoTracking()
                        .CountAsync(x =>
                            x.NgayChamSoc.HasValue
                            && x.NgayChamSoc.Value >= batDau
                            && x.NgayChamSoc.Value < ketThuc);

                decimal doanhThu =
                    await _context.DatLichs
                        .AsNoTracking()
                        .Where(x =>
                            x.TrangThai
                                == BookingStatus.DaHoanThanh
                            && x.NgayChamSoc.HasValue
                            && x.NgayChamSoc.Value >= batDau
                            && x.NgayChamSoc.Value < ketThuc)
                        .SumAsync(x => x.TongTien ?? 0);

                thongKeTheoThang.Add(
                    new AdminMonthlyStatisticViewModel
                    {
                        Thang = batDau.Month,
                        SoDatLich = soDatLich,
                        DoanhThu = doanhThu
                    });
            }

            var lichDatGanDay =
                await _context.DatLichs
                    .AsNoTracking()
                    .OrderByDescending(x => x.NgayDat)
                    .Take(8)
                    .Select(x =>
                        new AdminRecentBookingViewModel
                        {
                            MaDatLich =
                                x.MaDatLich,

                            TenKhachHang =
                                x.KhachHang != null
                                    ? x.KhachHang.HoTen
                                    : "Chưa cập nhật",

                            TenBenhNhan =
                                x.BenhNhan != null
                                    ? x.BenhNhan.HoTen
                                    : "Chưa cập nhật",

                            TenNguoiChamSoc =
                                x.NguoiChamSoc != null
                                    ? x.NguoiChamSoc.HoTen!
                                    : "Chưa phân công",

                            NgayChamSoc =
                                x.NgayChamSoc ?? homNay,

                            GioBatDau =
                                x.GioBatDau ?? TimeSpan.Zero,

                            GioKetThuc =
                                x.GioKetThuc ?? TimeSpan.Zero,

                            TongTien =
                                x.TongTien ?? 0,

                            TrangThai =
                                x.TrangThai ?? "Chưa xác định"
                        })
                    .ToListAsync();

            var hoSoChoDuyetGanDay =
                await _context.NguoiChamSocs
                    .AsNoTracking()
                    .Where(x =>
                        x.TrangThai == "Chờ duyệt")
                    .OrderByDescending(x => x.NgayTao)
                    .Take(4)
                    .Select(x =>
                        new AdminPendingCaregiverViewModel
                        {
                            MaNguoiChamSoc =
                                x.MaNguoiChamSoc,

                            HoTen =
                                x.HoTen ?? "Chưa cập nhật",

                            ChuyenMon =
                                x.ChuyenMon ?? "Chưa cập nhật",

                            KhuVucHoatDong =
                                x.KhuVucHoatDong
                                ?? "Chưa cập nhật",

                            KinhNghiem =
                                x.KinhNghiem ?? 0,

                            NgayTao =
                                x.NgayTao
                        })
                    .ToListAsync();

            var thongKeTrangThai =
                await _context.DatLichs
                    .AsNoTracking()
                    .GroupBy(x =>
                        x.TrangThai ?? "Khác")
                    .Select(g =>
                        new AdminBookingStatusViewModel
                        {
                            TrangThai = g.Key,
                            SoLuong = g.Count()
                        })
                    .OrderByDescending(x => x.SoLuong)
                    .ToListAsync();

            var model =
                new AdminDashboardViewModel
                {
                    TenQuanTriVien =
                        User.Identity?.Name
                        ?? "Quản trị viên",

                    TongTaiKhoan =
                        tongTaiKhoan,

                    TongKhachHang =
                        tongKhachHang,

                    TongNguoiChamSoc =
                        tongNguoiChamSoc,

                    HoSoChoDuyet =
                        hoSoChoDuyet,

                    LichChoXacNhan =
                        lichChoXacNhan,

                    TongDatLichThang =
                        tongDatLichThang,

                    TongDatLichHomNay =
                        tongDatLichHomNay,

                    DoanhThuThang =
                        doanhThuThang,

                    TyLeLapDay =
                        tyLeLapDay,

                    CoDuLieuLichLamViec =
                        coDuLieuLichLamViec,

                    DauTuan =
                        dauTuan,

                    CuoiTuan =
                        cuoiTuan,

                    TongDatLichTuan =
                        tongDatLichTuan,

                    LichHoanThanhTuan =
                        lichHoanThanhTuan,

                    LichDaHuyTuan =
                        lichDaHuyTuan,

                    TongGioMoLichTuan =
                        tongGioMoLichTuan,

                    TongGioDaDatTuan =
                        tongGioDaDatTuan,

                    ThongKeTheoNgayTrongTuan =
                        thongKeTuan,

                    ThongKeTheoThang =
                        thongKeTheoThang,

                    LichDatGanDay =
                        lichDatGanDay,

                    HoSoChoDuyetGanDay =
                        hoSoChoDuyetGanDay,

                    ThongKeTrangThai =
                        thongKeTrangThai
                };

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> XuatBaoCaoThang(
            int? nam,
            int? thang)
        {
            DateTime hienTai =
                VietnamClock.Now;

            int namBaoCao =
                nam ?? hienTai.Year;

            int thangBaoCao =
                thang ?? hienTai.Month;

            if (namBaoCao < 2000
                || namBaoCao > 2100
                || thangBaoCao < 1
                || thangBaoCao > 12)
            {
                return BadRequest(
                    "Tháng hoặc năm báo cáo không hợp lệ.");
            }

            DateTime dauThang =
                new DateTime(
                    namBaoCao,
                    thangBaoCao,
                    1);

            DateTime dauThangSau =
                dauThang.AddMonths(1);

            var danhSachLich =
                await (
                    from lich in _context.DatLichs
                        .AsNoTracking()

                    join khachHang in _context.KhachHangs
                        .AsNoTracking()
                        on lich.MaKhachHang
                        equals khachHang.MaKhachHang

                    join benhNhan in _context.BenhNhans
                        .AsNoTracking()
                        on lich.MaBenhNhan
                        equals benhNhan.MaBenhNhan

                    join dichVu in _context.DichVus
                        .AsNoTracking()
                        on lich.MaDichVu
                        equals dichVu.MaDichVu

                    join nguoiChamSocTam
                        in _context.NguoiChamSocs
                            .AsNoTracking()
                        on lich.MaNguoiChamSoc
                        equals nguoiChamSocTam.MaNguoiChamSoc
                        into nhomNguoiChamSoc

                    from nguoiChamSoc
                        in nhomNguoiChamSoc.DefaultIfEmpty()

                    where lich.NgayChamSoc.HasValue
                        && lich.NgayChamSoc.Value >= dauThang
                        && lich.NgayChamSoc.Value < dauThangSau

                    orderby
                        lich.NgayChamSoc,
                        lich.GioBatDau,
                        lich.MaDatLich

                    select new
                    {
                        lich.MaDatLich,

                        TenKhachHang =
                            string.IsNullOrWhiteSpace(
                                khachHang.HoTen)
                                ? "Chưa cập nhật"
                                : khachHang.HoTen,

                        TenBenhNhan =
                            string.IsNullOrWhiteSpace(
                                benhNhan.HoTen)
                                ? "Chưa cập nhật"
                                : benhNhan.HoTen,

                        TenNguoiChamSoc =
                            nguoiChamSoc == null
                            || string.IsNullOrWhiteSpace(
                                nguoiChamSoc.HoTen)
                                ? "Chưa phân công"
                                : nguoiChamSoc.HoTen,

                        TenDichVu =
                            dichVu.TenDichVu,

                        NgayChamSoc =
                            lich.NgayChamSoc.Value,

                        GioBatDau =
                            lich.GioBatDau
                            ?? TimeSpan.Zero,

                        GioKetThuc =
                            lich.GioKetThuc
                            ?? TimeSpan.Zero,

                        HinhThucPhanCong =
                            lich.HinhThucPhanCong,

                        TrangThai =
                            lich.TrangThai
                            ?? "Chưa xác định",

                        TongTien =
                            lich.TongTien
                            ?? 0m,

                        PhiNenTang =
                            lich.PhiNenTang
                            ?? 0m,

                        ThuNhapNguoiChamSoc =
                            lich.ThuNhapNguoiChamSoc
                            ?? 0m,

                        lich.ThoiDiemCheckIn,
                        lich.ThoiDiemCheckOut
                    })
                    .ToListAsync();

            int tongLich =
                danhSachLich.Count;

            int lichHoanThanh =
                danhSachLich.Count(x =>
                    x.TrangThai
                        == BookingStatus.DaHoanThanh);

            int lichDaHuy =
                danhSachLich.Count(x =>
                    x.TrangThai
                        == BookingStatus.DaHuy);

            decimal doanhThu =
                danhSachLich
                    .Where(x =>
                        x.TrangThai
                            == BookingStatus.DaHoanThanh)
                    .Sum(x =>
                        x.TongTien);

            decimal tongPhiNenTang =
                danhSachLich
                    .Where(x =>
                        x.TrangThai
                            == BookingStatus.DaHoanThanh)
                    .Sum(x =>
                        x.PhiNenTang);

            decimal tongThuNhapCaregiver =
                danhSachLich
                    .Where(x =>
                        x.TrangThai
                            == BookingStatus.DaHoanThanh)
                    .Sum(x =>
                        x.ThuNhapNguoiChamSoc);

            var csv =
                new StringBuilder();

            /*
             * BOM giúp Excel trên Windows đọc
             * tiếng Việt UTF-8 đúng dấu.
             */
            csv.Append('\uFEFF');

            ThemDongCsv(
                csv,
                "BÁO CÁO HOẠT ĐỘNG CARECONNECT");

            ThemDongCsv(
                csv,
                $"Tháng {thangBaoCao:00}/{namBaoCao}");

            ThemDongCsv(
                csv,
                $"Thời điểm xuất: {VietnamClock.Now:dd/MM/yyyy HH:mm:ss}");

            csv.AppendLine();

            ThemDongCsv(
                csv,
                "CHỈ SỐ TỔNG QUAN",
                "GIÁ TRỊ");

            ThemDongCsv(
                csv,
                "Tổng lượt đặt",
                tongLich);

            ThemDongCsv(
                csv,
                "Lịch đã hoàn thành",
                lichHoanThanh);

            ThemDongCsv(
                csv,
                "Lịch đã hủy",
                lichDaHuy);

            ThemDongCsv(
                csv,
                "Doanh thu lịch hoàn thành",
                doanhThu);

            ThemDongCsv(
                csv,
                "Phí nền tảng",
                tongPhiNenTang);

            ThemDongCsv(
                csv,
                "Thu nhập người chăm sóc",
                tongThuNhapCaregiver);

            csv.AppendLine();

            ThemDongCsv(
                csv,
                "Mã đặt lịch",
                "Khách hàng",
                "Người được chăm sóc",
                "Người chăm sóc",
                "Dịch vụ",
                "Ngày chăm sóc",
                "Giờ bắt đầu",
                "Giờ kết thúc",
                "Hình thức phân công",
                "Trạng thái",
                "Tổng tiền",
                "Phí nền tảng",
                "Thu nhập caregiver",
                "Check-in",
                "Check-out");

            foreach (var lich
                     in danhSachLich)
            {
                ThemDongCsv(
                    csv,
                    $"DL{lich.MaDatLich}",
                    lich.TenKhachHang,
                    lich.TenBenhNhan,
                    lich.TenNguoiChamSoc,
                    lich.TenDichVu,
                    lich.NgayChamSoc
                        .ToString("dd/MM/yyyy"),
                    lich.GioBatDau
                        .ToString(@"hh\:mm"),
                    lich.GioKetThuc
                        .ToString(@"hh\:mm"),
                    lich.HinhThucPhanCong,
                    lich.TrangThai,
                    lich.TongTien,
                    lich.PhiNenTang,
                    lich.ThuNhapNguoiChamSoc,
                    lich.ThoiDiemCheckIn
                        ?.ToString("dd/MM/yyyy HH:mm:ss")
                        ?? string.Empty,
                    lich.ThoiDiemCheckOut
                        ?.ToString("dd/MM/yyyy HH:mm:ss")
                        ?? string.Empty);
            }

            byte[] noiDung =
                Encoding.UTF8.GetBytes(
                    csv.ToString());

            string tenFile =
                $"CareConnect_BaoCao_{thangBaoCao:00}-{namBaoCao}.csv";

            return File(
                noiDung,
                "text/csv; charset=utf-8",
                tenFile);
        }

        private static void ThemDongCsv(
            StringBuilder builder,
            params object?[] giaTri)
        {
            builder.AppendLine(
                string.Join(
                    ",",
                    giaTri.Select(
                        ChuyenGiaTriCsv)));
        }

        private static string ChuyenGiaTriCsv(
            object? giaTri)
        {
            string text =
                giaTri?.ToString()
                ?? string.Empty;

            /*
             * Tránh Excel hiểu dữ liệu người dùng
             * là công thức.
             */
            if (text.StartsWith("=")
                || text.StartsWith("+")
                || text.StartsWith("-")
                || text.StartsWith("@"))
            {
                text =
                    "'" + text;
            }

            return "\""
                + text.Replace(
                    "\"",
                    "\"\"")
                + "\"";
        }

        private static List<AdminDailyStatisticViewModel>
            TaoThongKeTheoNgayTrongTuan(
                DateTime dauTuan,
                List<DatLich> datLichTrongTuan,
                List<LichLamViec> lichLamViecTrongTuan)
        {
            var ketQua =
                new List<AdminDailyStatisticViewModel>();

            for (int i = 0; i < 7; i++)
            {
                DateTime ngay =
                    dauTuan.AddDays(i).Date;

                var datLichTrongNgay =
                    datLichTrongTuan
                        .Where(x =>
                            x.NgayChamSoc.HasValue
                            && x.NgayChamSoc.Value.Date == ngay)
                        .ToList();

                var lichLamViecTrongNgay =
                    lichLamViecTrongTuan
                        .Where(x =>
                            x.NgayLam.Date == ngay
                            && LaKhungLamViecKhaDung(x))
                        .ToList();

                var caregiverIds =
                    lichLamViecTrongNgay
                        .Select(x => x.MaNguoiChamSoc)
                        .Distinct()
                        .ToList();

                double tongPhutMoLich = 0;
                double tongPhutDaDat = 0;

                foreach (int maNguoiChamSoc
                         in caregiverIds)
                {
                    var khungMoLich =
                        lichLamViecTrongNgay
                            .Where(x =>
                                x.MaNguoiChamSoc
                                    == maNguoiChamSoc)
                            .Select(x =>
                                new TimeInterval(
                                    x.GioBatDau,
                                    x.GioKetThuc));

                    var khungMoLichDaGop =
                        GopKhoangThoiGian(khungMoLich);

                    tongPhutMoLich +=
                        TinhTongSoPhut(khungMoLichDaGop);

                    var khungDat =
                        datLichTrongNgay
                            .Where(x =>
                                x.MaNguoiChamSoc
                                    == maNguoiChamSoc
                                && x.TrangThai
                                    != BookingStatus.DaHuy
                                && x.GioBatDau.HasValue
                                && x.GioKetThuc.HasValue
                                && x.GioKetThuc.Value
                                    > x.GioBatDau.Value)
                            .Select(x =>
                                new TimeInterval(
                                    x.GioBatDau!.Value,
                                    x.GioKetThuc!.Value))
                            .ToList();

                    var khungDaDatTrongLichMo =
                        LayPhanGiaoNhau(
                            khungDat,
                            khungMoLichDaGop);

                    tongPhutDaDat +=
                        TinhTongSoPhut(
                            GopKhoangThoiGian(
                                khungDaDatTrongLichMo));
                }

                double gioMoLich =
                    Math.Round(
                        tongPhutMoLich / 60.0,
                        1);

                double gioDaDat =
                    Math.Round(
                        tongPhutDaDat / 60.0,
                        1);

                double? tyLeLapDay =
                    tongPhutMoLich <= 0
                        ? null
                        : Math.Round(
                            Math.Min(
                                100,
                                tongPhutDaDat
                                    * 100.0
                                    / tongPhutMoLich),
                            1);

                ketQua.Add(
                    new AdminDailyStatisticViewModel
                    {
                        Ngay = ngay,
                        NhanNgay = TaoNhanNgay(ngay),
                        SoDatLich = datLichTrongNgay.Count,
                        SoHoanThanh =
                            datLichTrongNgay.Count(x =>
                                x.TrangThai
                                    == BookingStatus.DaHoanThanh),
                        SoDaHuy =
                            datLichTrongNgay.Count(x =>
                                x.TrangThai
                                    == BookingStatus.DaHuy),
                        GioMoLich = gioMoLich,
                        GioDaDat = gioDaDat,
                        TyLeLapDay = tyLeLapDay
                    });
            }

            return ketQua;
        }

        private static bool LaKhungLamViecKhaDung(
            LichLamViec lich)
        {
            if (lich.GioKetThuc <= lich.GioBatDau)
            {
                return false;
            }

            string trangThai =
                lich.TrangThai?.Trim()
                ?? string.Empty;

            string[] trangThaiKhongTinh =
            {
                "Không khả dụng",
                "Nghỉ",
                "Tạm nghỉ",
                "Đã hủy"
            };

            return !trangThaiKhongTinh.Contains(
                trangThai,
                StringComparer.OrdinalIgnoreCase);
        }

        private static string TaoNhanNgay(
            DateTime ngay)
        {
            string thu =
                ngay.DayOfWeek switch
                {
                    DayOfWeek.Monday => "T2",
                    DayOfWeek.Tuesday => "T3",
                    DayOfWeek.Wednesday => "T4",
                    DayOfWeek.Thursday => "T5",
                    DayOfWeek.Friday => "T6",
                    DayOfWeek.Saturday => "T7",
                    _ => "CN"
                };

            return $"{thu} {ngay:dd/MM}";
        }

        private static List<TimeInterval>
            GopKhoangThoiGian(
                IEnumerable<TimeInterval> khoangThoiGian)
        {
            var danhSach =
                khoangThoiGian
                    .Where(x => x.End > x.Start)
                    .OrderBy(x => x.Start)
                    .ThenBy(x => x.End)
                    .ToList();

            if (danhSach.Count == 0)
            {
                return new List<TimeInterval>();
            }

            var ketQua =
                new List<TimeInterval>();

            TimeInterval hienTai =
                danhSach[0];

            for (int i = 1;
                 i < danhSach.Count;
                 i++)
            {
                TimeInterval tiepTheo =
                    danhSach[i];

                if (tiepTheo.Start <= hienTai.End)
                {
                    if (tiepTheo.End > hienTai.End)
                    {
                        hienTai =
                            new TimeInterval(
                                hienTai.Start,
                                tiepTheo.End);
                    }
                }
                else
                {
                    ketQua.Add(hienTai);
                    hienTai = tiepTheo;
                }
            }

            ketQua.Add(hienTai);

            return ketQua;
        }

        private static List<TimeInterval>
            LayPhanGiaoNhau(
                IEnumerable<TimeInterval> lichDat,
                IEnumerable<TimeInterval> lichMo)
        {
            var ketQua =
                new List<TimeInterval>();

            foreach (TimeInterval dat
                     in lichDat)
            {
                foreach (TimeInterval mo
                         in lichMo)
                {
                    TimeSpan batDau =
                        dat.Start > mo.Start
                            ? dat.Start
                            : mo.Start;

                    TimeSpan ketThuc =
                        dat.End < mo.End
                            ? dat.End
                            : mo.End;

                    if (ketThuc > batDau)
                    {
                        ketQua.Add(
                            new TimeInterval(
                                batDau,
                                ketThuc));
                    }
                }
            }

            return ketQua;
        }

        private static double TinhTongSoPhut(
            IEnumerable<TimeInterval> intervals)
        {
            return intervals.Sum(x =>
                (x.End - x.Start).TotalMinutes);
        }

        private sealed class TimeInterval
        {
            public TimeInterval(
                TimeSpan start,
                TimeSpan end)
            {
                Start = start;
                End = end;
            }

            public TimeSpan Start { get; }

            public TimeSpan End { get; }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DuyetNguoiChamSoc(
            int maNguoiChamSoc)
        {
            var nguoiChamSoc =
                await _context.NguoiChamSocs
                    .FirstOrDefaultAsync(x =>
                        x.MaNguoiChamSoc
                        == maNguoiChamSoc);

            if (nguoiChamSoc == null)
            {
                TempData["Error"] =
                    "Không tìm thấy hồ sơ người chăm sóc.";

                return RedirectToAction(nameof(Index));
            }

            nguoiChamSoc.TrangThai =
                "Đã duyệt";

            if (nguoiChamSoc.MaTaiKhoan.HasValue)
            {
                var taiKhoan =
                    await _context.TaiKhoans
                        .FirstOrDefaultAsync(x =>
                            x.MaTaiKhoan
                            == nguoiChamSoc.MaTaiKhoan.Value);

                if (taiKhoan != null)
                {
                    taiKhoan.TrangThai = true;
                }
            }

            await _context.SaveChangesAsync();

            TempData["Success"] =
                $"Đã duyệt hồ sơ của {nguoiChamSoc.HoTen}.";

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> TuChoiNguoiChamSoc(
            int maNguoiChamSoc)
        {
            var nguoiChamSoc =
                await _context.NguoiChamSocs
                    .FirstOrDefaultAsync(x =>
                        x.MaNguoiChamSoc
                        == maNguoiChamSoc);

            if (nguoiChamSoc == null)
            {
                TempData["Error"] =
                    "Không tìm thấy hồ sơ người chăm sóc.";

                return RedirectToAction(nameof(Index));
            }

            nguoiChamSoc.TrangThai =
                "Từ chối";

            if (nguoiChamSoc.MaTaiKhoan.HasValue)
            {
                var taiKhoan =
                    await _context.TaiKhoans
                        .FirstOrDefaultAsync(x =>
                            x.MaTaiKhoan
                            == nguoiChamSoc.MaTaiKhoan.Value);

                if (taiKhoan != null)
                {
                    taiKhoan.TrangThai = false;
                }
            }

            await _context.SaveChangesAsync();

            TempData["Success"] =
                $"Đã từ chối hồ sơ của {nguoiChamSoc.HoTen}.";

            return RedirectToAction(nameof(Index));
        }
    }
}
