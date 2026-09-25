using System.Security.Claims;
using CareConnect.Constants;
using CareConnect.Data;
using CareConnect.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CareConnect.Areas.Caregiver.Controllers
{
    [Area("Caregiver")]
    [Authorize(Roles = "Người chăm sóc")]
    public class ThuNhapController : Controller
    {
        private readonly CareConnectDbContext _context;

        public ThuNhapController(
            CareConnectDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Index(
            string? tuKhoa,
            string? trangThai,
            DateTime? tuNgay,
            DateTime? denNgay,
            string? locNhanh)
        {
            int? maTaiKhoan =
                LayMaTaiKhoan();

            if (!maTaiKhoan.HasValue)
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
                        x.MaTaiKhoan
                            == maTaiKhoan.Value);

            if (nguoiChamSoc == null)
            {
                return NotFound(
                    "Không tìm thấy hồ sơ người chăm sóc.");
            }

            int maNguoiChamSoc =
                nguoiChamSoc.MaNguoiChamSoc;

            string tuKhoaDaChuanHoa =
                tuKhoa?.Trim()
                ?? string.Empty;

            string trangThaiDaChuanHoa =
                trangThai?.Trim()
                ?? string.Empty;

            string locNhanhDaChuanHoa =
                locNhanh?.Trim().ToLowerInvariant()
                ?? string.Empty;

            /*
             * Danh sách phía dưới vẫn dựa trên bản ghi ThanhToan.
             * Tuy nhiên SoTien hiển thị là phần người chăm sóc
             * được nhận sau khi trừ phí nền tảng, không phải
             * toàn bộ số tiền khách hàng đã thanh toán.
             */
            var query =
                from thanhToan in
                    _context.ThanhToans.AsNoTracking()

                join datLich in
                    _context.DatLichs.AsNoTracking()
                    on thanhToan.MaDatLich
                    equals datLich.MaDatLich

                join khachHang in
                    _context.KhachHangs.AsNoTracking()
                    on datLich.MaKhachHang
                    equals khachHang.MaKhachHang

                join benhNhan in
                    _context.BenhNhans.AsNoTracking()
                    on datLich.MaBenhNhan
                    equals benhNhan.MaBenhNhan

                join dichVu in
                    _context.DichVus.AsNoTracking()
                    on datLich.MaDichVu
                    equals dichVu.MaDichVu

                where
                    datLich.MaNguoiChamSoc
                        == maNguoiChamSoc

                select new
                {
                    ThanhToan = thanhToan,
                    DatLich = datLich,
                    KhachHang = khachHang,
                    BenhNhan = benhNhan,
                    DichVu = dichVu
                };

            if (!string.IsNullOrWhiteSpace(
                    tuKhoaDaChuanHoa))
            {
                query =
                    query.Where(x =>
                        x.KhachHang.HoTen.Contains(
                            tuKhoaDaChuanHoa)
                        ||
                        x.BenhNhan.HoTen.Contains(
                            tuKhoaDaChuanHoa)
                        ||
                        x.DichVu.TenDichVu.Contains(
                            tuKhoaDaChuanHoa)
                        ||
                        (
                            x.ThanhToan.MaThanhToanCode != null
                            &&
                            x.ThanhToan.MaThanhToanCode.Contains(
                                tuKhoaDaChuanHoa)
                        ));
            }

            if (!string.IsNullOrWhiteSpace(
                    trangThaiDaChuanHoa))
            {
                query =
                    query.Where(x =>
                        x.ThanhToan.TrangThai
                            == trangThaiDaChuanHoa);
            }

            if (locNhanhDaChuanHoa
                == "da-thanh-toan")
            {
                query =
                    query.Where(x =>
                        x.ThanhToan.TrangThai
                            == "Đã thanh toán");
            }
            else if (locNhanhDaChuanHoa
                == "cho-thanh-toan")
            {
                query =
                    query.Where(x =>
                        x.ThanhToan.TrangThai
                            != "Đã thanh toán");
            }
            else if (locNhanhDaChuanHoa
                == "thang-nay")
            {
                DateTime dauThang =
                    new DateTime(
                        DateTime.Today.Year,
                        DateTime.Today.Month,
                        1);

                DateTime dauThangSau =
                    dauThang.AddMonths(1);

                query =
                    query.Where(x =>
                        x.ThanhToan.TrangThai
                            == "Đã thanh toán"
                        &&
                        x.ThanhToan.NgayThanhToan.HasValue
                        &&
                        x.ThanhToan.NgayThanhToan.Value
                            >= dauThang
                        &&
                        x.ThanhToan.NgayThanhToan.Value
                            < dauThangSau);
            }

            if (tuNgay.HasValue)
            {
                DateTime ngayBatDau =
                    tuNgay.Value.Date;

                query =
                    query.Where(x =>
                        (
                            x.ThanhToan.NgayThanhToan
                            ?? x.ThanhToan.NgayTao
                        ).Date >= ngayBatDau);
            }

            if (denNgay.HasValue)
            {
                DateTime ngayKetThuc =
                    denNgay.Value.Date;

                query =
                    query.Where(x =>
                        (
                            x.ThanhToan.NgayThanhToan
                            ?? x.ThanhToan.NgayTao
                        ).Date <= ngayKetThuc);
            }

            var duLieuGiaoDich =
                await query
                    .OrderByDescending(x =>
                        x.ThanhToan.NgayThanhToan
                        ?? x.ThanhToan.NgayTao)
                    .ThenByDescending(x =>
                        x.ThanhToan.MaThanhToan)
                    .Select(x =>
                        new
                        {
                            x.ThanhToan.MaThanhToan,

                            MaThanhToanCode =
                                x.ThanhToan.MaThanhToanCode,

                            x.DatLich.MaDatLich,

                            TenKhachHang =
                                x.KhachHang.HoTen,

                            TenBenhNhan =
                                x.BenhNhan.HoTen,

                            TenDichVu =
                                x.DichVu.TenDichVu,

                            NgayChamSoc =
                                x.DatLich.NgayChamSoc,

                            SoTienKhachDaThanhToan =
                                x.ThanhToan.SoTien,

                            TyLePhiNenTang =
                                x.DatLich.TyLePhiNenTang
                                ?? PaymentPolicy.PlatformFeePercent,

                            PhuongThucThanhToan =
                                x.ThanhToan.PhuongThucThanhToan,

                            TrangThaiThanhToan =
                                x.ThanhToan.TrangThai,

                            x.ThanhToan.NgayThanhToan,

                            x.ThanhToan.NgayTao
                        })
                    .ToListAsync();

            var danhSachGiaoDich =
                duLieuGiaoDich
                    .Select(x =>
                        new CaregiverIncomeItemViewModel
                        {
                            MaThanhToan =
                                x.MaThanhToan,

                            MaThanhToanCode =
                                x.MaThanhToanCode
                                ?? $"TT{x.MaThanhToan}",

                            MaDatLich =
                                x.MaDatLich,

                            TenKhachHang =
                                x.TenKhachHang,

                            TenBenhNhan =
                                x.TenBenhNhan,

                            TenDichVu =
                                x.TenDichVu,

                            NgayChamSoc =
                                x.NgayChamSoc
                                ?? DateTime.MinValue,

                            SoTien =
                                TinhThuNhapTuSoTienKhachDaTra(
                                    x.SoTienKhachDaThanhToan,
                                    x.TyLePhiNenTang),

                            PhuongThucThanhToan =
                                x.PhuongThucThanhToan
                                ?? "Chưa cập nhật",

                            TrangThai =
                                x.TrangThaiThanhToan
                                ?? "Chưa thanh toán",

                            NgayThanhToan =
                                x.NgayThanhToan,

                            NgayTao =
                                x.NgayTao
                        })
                    .ToList();

            /*
             * Lấy toàn bộ lịch đã phân công cho caregiver,
             * kể cả lịch chưa có bản ghi thanh toán.
             * Nhờ đó "Đang chờ" phản ánh phần thu nhập
             * thực sự chưa được khách thanh toán.
             */
            var duLieuThuNhap =
                await (
                    from datLich in
                        _context.DatLichs.AsNoTracking()

                    join thanhToanTam in
                        _context.ThanhToans.AsNoTracking()
                        on datLich.MaDatLich
                        equals thanhToanTam.MaDatLich
                        into nhomThanhToan

                    from thanhToan in
                        nhomThanhToan.DefaultIfEmpty()

                    where
                        datLich.MaNguoiChamSoc
                            == maNguoiChamSoc

                    select new
                    {
                        datLich.MaDatLich,

                        TrangThaiLich =
                            datLich.TrangThai,

                        TongTien =
                            datLich.TongTien ?? 0,

                        TyLePhiNenTang =
                            datLich.TyLePhiNenTang
                            ?? PaymentPolicy.PlatformFeePercent,

                        ThuNhapDuKien =
                            datLich.ThuNhapNguoiChamSoc
                            ??
                            (
                                (datLich.TongTien ?? 0)
                                *
                                (
                                    100m
                                    -
                                    (
                                        datLich.TyLePhiNenTang
                                        ?? PaymentPolicy.PlatformFeePercent
                                    )
                                )
                                / 100m
                            ),

                        SoTienKhachDaThanhToan =
                            thanhToan == null
                                ? 0
                                : thanhToan.SoTien,

                        TrangThaiThanhToan =
                            thanhToan == null
                                ? "Chưa thanh toán"
                                : thanhToan.TrangThai,

                        NgayThanhToan =
                            thanhToan == null
                                ? null
                                : thanhToan.NgayThanhToan
                    })
                .ToListAsync();

            decimal tongThuNhap =
                duLieuThuNhap
                    .Where(x =>
                        x.TrangThaiThanhToan
                            == "Đã thanh toán")
                    .Sum(x =>
                        x.ThuNhapDuKien);

            DateTime dauThangHienTai =
                new DateTime(
                    DateTime.Today.Year,
                    DateTime.Today.Month,
                    1);

            DateTime dauThangTiepTheo =
                dauThangHienTai.AddMonths(1);

            decimal thuNhapThangNay =
                duLieuThuNhap
                    .Where(x =>
                        x.TrangThaiThanhToan
                            == "Đã thanh toán"
                        &&
                        x.NgayThanhToan.HasValue
                        &&
                        x.NgayThanhToan.Value
                            >= dauThangHienTai
                        &&
                        x.NgayThanhToan.Value
                            < dauThangTiepTheo)
                    .Sum(x =>
                        x.ThuNhapDuKien);

            decimal dangChoThanhToan =
                duLieuThuNhap
                    .Where(x =>
                        x.TrangThaiLich
                            != "Đã hủy"
                        &&
                        x.TrangThaiThanhToan
                            != "Đã thanh toán")
                    .Sum(x =>
                    {
                        decimal daGhiNhan =
                            TinhThuNhapTuSoTienKhachDaTra(
                                x.SoTienKhachDaThanhToan,
                                x.TyLePhiNenTang);

                        return Math.Max(
                            0,
                            x.ThuNhapDuKien
                            - daGhiNhan);
                    });

            int soGiaoDichDaThanhToan =
                duLieuThuNhap.Count(x =>
                    x.TrangThaiThanhToan
                        == "Đã thanh toán");

            int soGiaoDichChoThanhToan =
                duLieuThuNhap.Count(x =>
                    x.TrangThaiLich
                        != "Đã hủy"
                    &&
                    x.TrangThaiThanhToan
                        != "Đã thanh toán");

            var thuNhapTheoThang =
                TaoThuNhapTheoThang(
                    duLieuThuNhap.Select(x =>
                        (
                            NgayThanhToan:
                                x.NgayThanhToan,

                            TrangThai:
                                x.TrangThaiThanhToan,

                            ThuNhap:
                                x.ThuNhapDuKien
                        )));

            var model =
                new CaregiverIncomeViewModel
                {
                    TongThuNhap =
                        tongThuNhap,

                    ThuNhapThangNay =
                        thuNhapThangNay,

                    DangChoThanhToan =
                        dangChoThanhToan,

                    SoGiaoDichDaThanhToan =
                        soGiaoDichDaThanhToan,

                    SoGiaoDichChoThanhToan =
                        soGiaoDichChoThanhToan,

                    TuKhoa =
                        tuKhoaDaChuanHoa,

                    TrangThai =
                        trangThaiDaChuanHoa,

                    LocNhanh =
                        locNhanhDaChuanHoa,

                    TuNgay =
                        tuNgay,

                    DenNgay =
                        denNgay,

                    ThuNhapTheoThang =
                        thuNhapTheoThang,

                    DanhSachGiaoDich =
                        danhSachGiaoDich
                };

            return View(model);
        }

        // =====================================================
        // TÍNH PHẦN NGƯỜI CHĂM SÓC ĐƯỢC NHẬN
        // TỪ SỐ TIỀN KHÁCH ĐÃ THANH TOÁN.
        // =====================================================
        private static decimal
            TinhThuNhapTuSoTienKhachDaTra(
                decimal soTienKhachDaTra,
                decimal tyLePhiNenTang)
        {
            decimal tyLePhi =
                Math.Clamp(
                    tyLePhiNenTang,
                    0m,
                    100m);

            return decimal.Round(
                soTienKhachDaTra
                * (100m - tyLePhi)
                / 100m,
                0,
                MidpointRounding.AwayFromZero);
        }

        private static List<CaregiverMonthlyIncomeViewModel>
            TaoThuNhapTheoThang(
                IEnumerable<
                    (
                        DateTime? NgayThanhToan,
                        string? TrangThai,
                        decimal ThuNhap
                    )>
                    danhSachThuNhap)
        {
            DateTime thangHienTai =
                new DateTime(
                    DateTime.Today.Year,
                    DateTime.Today.Month,
                    1);

            var ketQua =
                new List<CaregiverMonthlyIncomeViewModel>();

            for (int i = 5; i >= 0; i--)
            {
                DateTime thang =
                    thangHienTai.AddMonths(-i);

                DateTime thangSau =
                    thang.AddMonths(1);

                decimal soTien =
                    danhSachThuNhap
                        .Where(x =>
                            x.TrangThai
                                == "Đã thanh toán"
                            &&
                            x.NgayThanhToan.HasValue
                            &&
                            x.NgayThanhToan.Value
                                >= thang
                            &&
                            x.NgayThanhToan.Value
                                < thangSau)
                        .Sum(x =>
                            x.ThuNhap);

                ketQua.Add(
                    new CaregiverMonthlyIncomeViewModel
                    {
                        Thang =
                            thang.Month,

                        Nam =
                            thang.Year,

                        NhanThang =
                            $"T{thang.Month}/{thang.Year}",

                        SoTien =
                            soTien
                    });
            }

            decimal caoNhat =
                ketQua.Count > 0
                    ? ketQua.Max(x =>
                        x.SoTien)
                    : 0;

            foreach (var item
                     in ketQua)
            {
                item.PhanTram =
                    caoNhat > 0
                        ? (int)Math.Round(
                            item.SoTien
                            * 100
                            / caoNhat)
                        : 0;
            }

            return ketQua;
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
