using CareConnect.Constants;
using CareConnect.Data;
using CareConnect.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace CareConnect.Areas.Caregiver.Controllers
{
    [Area("Caregiver")]
    [Authorize(Roles = "Người chăm sóc")]
    public class LichSuDanhGiaController : Controller
    {
        private readonly CareConnectDbContext _context;

        public LichSuDanhGiaController(
            CareConnectDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Index(
            string? tuKhoa,
            int? soSao,
            DateTime? tuNgay,
            DateTime? denNgay)
        {
            int? maTaiKhoan = LayMaTaiKhoan();

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
                        x.MaTaiKhoan == maTaiKhoan.Value);

            if (nguoiChamSoc == null)
            {
                return NotFound(
                    "Không tìm thấy hồ sơ người chăm sóc.");
            }

            int maNguoiChamSoc =
                nguoiChamSoc.MaNguoiChamSoc;

            string tuKhoaDaChuanHoa =
                tuKhoa?.Trim() ?? string.Empty;

            var danhGiaQuery =
                from danhGia in
                    _context.DanhGias.AsNoTracking()

                join datLich in
                    _context.DatLichs.AsNoTracking()

                    on danhGia.MaDatLich
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

                where datLich.MaNguoiChamSoc
                    == maNguoiChamSoc

                select new
                {
                    DanhGia = danhGia,
                    DatLich = datLich,
                    KhachHang = khachHang,
                    BenhNhan = benhNhan,
                    DichVu = dichVu
                };

            if (!string.IsNullOrWhiteSpace(
                    tuKhoaDaChuanHoa))
            {
                danhGiaQuery =
                    danhGiaQuery.Where(x =>
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
                            x.DanhGia.NoiDung != null
                            &&
                            x.DanhGia.NoiDung.Contains(
                                tuKhoaDaChuanHoa)
                        ));
            }

            if (soSao.HasValue)
            {
                danhGiaQuery =
                    danhGiaQuery.Where(x =>
                        x.DanhGia.SoSao
                            == soSao.Value);
            }

            if (tuNgay.HasValue)
            {
                DateTime ngayBatDau =
                    tuNgay.Value.Date;

                danhGiaQuery =
                    danhGiaQuery.Where(x =>
                        x.DanhGia.NgayDanhGia.Date
                            >= ngayBatDau);
            }

            if (denNgay.HasValue)
            {
                DateTime ngayKetThuc =
                    denNgay.Value.Date;

                danhGiaQuery =
                    danhGiaQuery.Where(x =>
                        x.DanhGia.NgayDanhGia.Date
                            <= ngayKetThuc);
            }

            var danhSachDanhGia =
                await danhGiaQuery
                    .OrderByDescending(x =>
                        x.DanhGia.NgayDanhGia)
                    .Select(x =>
                        new CaregiverReviewItemViewModel
                        {
                            MaDanhGia =
                                x.DanhGia.MaDanhGia,

                            MaDatLich =
                                x.DatLich.MaDatLich,

                            TenKhachHang =
                                x.KhachHang.HoTen,

                            TenBenhNhan =
                                x.BenhNhan.HoTen,

                            TenDichVu =
                                x.DichVu.TenDichVu,

                            SoSao =
                                x.DanhGia.SoSao,

                            NoiDung =
                                x.DanhGia.NoiDung
                                ?? string.Empty,

                            NgayDanhGia =
                                x.DanhGia.NgayDanhGia,

                            NgayChamSoc =
                                x.DatLich.NgayChamSoc
                                ?? DateTime.MinValue
                        })
                    .ToListAsync();

            var tatCaDanhGia =
                from danhGia in
                    _context.DanhGias.AsNoTracking()

                join datLich in
                    _context.DatLichs.AsNoTracking()

                    on danhGia.MaDatLich
                    equals datLich.MaDatLich

                where datLich.MaNguoiChamSoc
                    == maNguoiChamSoc

                select danhGia;

            int tongDanhGia =
                await tatCaDanhGia.CountAsync();

            double diemTrungBinh =
                tongDanhGia > 0
                    ? await tatCaDanhGia.AverageAsync(x =>
                        (double)x.SoSao)
                    : 0;

            int namSao =
                await tatCaDanhGia.CountAsync(x =>
                    x.SoSao == 5);

            int bonSao =
                await tatCaDanhGia.CountAsync(x =>
                    x.SoSao == 4);

            int baSao =
                await tatCaDanhGia.CountAsync(x =>
                    x.SoSao == 3);

            int haiSao =
                await tatCaDanhGia.CountAsync(x =>
                    x.SoSao == 2);

            int motSao =
                await tatCaDanhGia.CountAsync(x =>
                    x.SoSao == 1);

            int tyLeNamSao =
                tongDanhGia > 0
                    ? (int)Math.Round(
                        namSao * 100.0 / tongDanhGia)
                    : 0;

            var lichSuChamSoc =
                await (
                    from datLich in
                        _context.DatLichs.AsNoTracking()

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

                    join danhGia in
                        _context.DanhGias.AsNoTracking()

                        on datLich.MaDatLich
                        equals danhGia.MaDatLich
                        into danhGiaGroup

                    from danhGia in
                        danhGiaGroup.DefaultIfEmpty()

                    where datLich.MaNguoiChamSoc
                            == maNguoiChamSoc
                        &&
                        datLich.TrangThai
                            == BookingStatus.DaHoanThanh

                    orderby datLich.NgayChamSoc descending,
                        datLich.GioBatDau descending

                    select new CaregiverHistoryItemViewModel
                    {
                        MaDatLich =
                            datLich.MaDatLich,

                        TenBenhNhan =
                            benhNhan.HoTen,

                        TenKhachHang =
                            khachHang.HoTen,

                        TenDichVu =
                            dichVu.TenDichVu,

                        NgayChamSoc =
                            datLich.NgayChamSoc
                            ?? DateTime.MinValue,

                        GioBatDau =
                            datLich.GioBatDau
                            ?? TimeSpan.Zero,

                        GioKetThuc =
                            datLich.GioKetThuc
                            ?? TimeSpan.Zero,

                        DiaChiChamSoc =
                            datLich.DiaChiChamSoc
                            ?? "Chưa cập nhật",

                        TongTien =
                            datLich.TongTien
                            ?? 0,

                        SoSao =
                            danhGia != null
                                ? danhGia.SoSao
                                : null,

                        TrangThai =
                            datLich.TrangThai
                            ?? "Đã hoàn thành"
                    }
                )
                .Take(10)
                .ToListAsync();

            int tongBuoiHoanThanh =
                await _context.DatLichs
                    .AsNoTracking()
                    .CountAsync(x =>
                        x.MaNguoiChamSoc
                            == maNguoiChamSoc
                        &&
                        x.TrangThai
                            == "Đã hoàn thành");

            var model =
                new CaregiverReviewViewModel
                {
                    DiemTrungBinh =
                        diemTrungBinh,

                    TongDanhGia =
                        tongDanhGia,

                    TyLeNamSao =
                        tyLeNamSao,

                    TongBuoiHoanThanh =
                        tongBuoiHoanThanh,

                    NamSao =
                        namSao,

                    BonSao =
                        bonSao,

                    BaSao =
                        baSao,

                    HaiSao =
                        haiSao,

                    MotSao =
                        motSao,

                    TuKhoa =
                        tuKhoaDaChuanHoa,

                    SoSao =
                        soSao,

                    TuNgay =
                        tuNgay,

                    DenNgay =
                        denNgay,

                    DanhSachDanhGia =
                        danhSachDanhGia,

                    LichSuChamSoc =
                        lichSuChamSoc
                };

            return View(model);
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