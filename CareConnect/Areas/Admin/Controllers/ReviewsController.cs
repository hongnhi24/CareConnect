using CareConnect.Data;
using CareConnect.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CareConnect.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Quản trị viên")]
    public class ReviewsController : Controller
    {
        private readonly CareConnectDbContext _context;

        public ReviewsController(
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
            tuKhoa =
                tuKhoa?.Trim()
                ?? string.Empty;

            /*
             * Lấy toàn bộ dữ liệu đánh giá và nối qua lịch đặt.
             */
            var query =
                from danhGia in _context.DanhGias
                    .AsNoTracking()

                join lich in _context.DatLichs
                    .AsNoTracking()
                    on danhGia.MaDatLich
                    equals lich.MaDatLich

                join khachHang in _context.KhachHangs
                    .AsNoTracking()
                    on lich.MaKhachHang
                    equals khachHang.MaKhachHang

                join benhNhan in _context.BenhNhans
                    .AsNoTracking()
                    on lich.MaBenhNhan
                    equals benhNhan.MaBenhNhan

                join nguoiChamSocTam
                    in _context.NguoiChamSocs
                        .AsNoTracking()
                    on lich.MaNguoiChamSoc
                    equals nguoiChamSocTam.MaNguoiChamSoc
                    into nhomNguoiChamSoc

                from nguoiChamSoc
                    in nhomNguoiChamSoc.DefaultIfEmpty()

                join dichVuTam
                    in _context.DichVus
                        .AsNoTracking()
                    on lich.MaDichVu
                    equals dichVuTam.MaDichVu
                    into nhomDichVu

                from dichVu
                    in nhomDichVu.DefaultIfEmpty()

                select new AdminReviewItemViewModel
                {
                    MaDanhGia =
                        danhGia.MaDanhGia,

                    MaDatLich =
                        danhGia.MaDatLich,

                    SoSao =
                        danhGia.SoSao,

                    NoiDung =
                        danhGia.NoiDung
                        ?? "Khách hàng không để lại nhận xét.",

                    NgayDanhGia =
                        danhGia.NgayDanhGia,

                    TenKhachHang =
                        string.IsNullOrWhiteSpace(
                            khachHang.HoTen)
                            ? "Khách hàng"
                            : khachHang.HoTen,

                    TenNguoiChamSoc =
                        nguoiChamSoc == null ||
                        string.IsNullOrWhiteSpace(
                            nguoiChamSoc.HoTen)
                            ? "Chưa phân công"
                            : nguoiChamSoc.HoTen,

                    TenBenhNhan =
                        string.IsNullOrWhiteSpace(
                            benhNhan.HoTen)
                            ? "Người được chăm sóc"
                            : benhNhan.HoTen,

                    TenDichVu =
                        dichVu == null ||
                        string.IsNullOrWhiteSpace(
                            dichVu.TenDichVu)
                            ? "Chưa cập nhật"
                            : dichVu.TenDichVu,

                    NgayChamSoc =
                        lich.NgayChamSoc
                        ?? DateTime.Today,

                    TrangThaiLich =
                        lich.TrangThai
                        ?? "Chưa xác định"
                };

            /*
             * Lọc theo từ khóa.
             */
            if (!string.IsNullOrWhiteSpace(tuKhoa))
            {
                query = query.Where(x =>
                    x.TenKhachHang.Contains(tuKhoa)
                    ||
                    x.TenNguoiChamSoc.Contains(tuKhoa)
                    ||
                    x.TenBenhNhan.Contains(tuKhoa)
                    ||
                    x.TenDichVu.Contains(tuKhoa)
                    ||
                    x.NoiDung.Contains(tuKhoa)
                    ||
                    x.MaDatLich.ToString().Contains(tuKhoa));
            }

            /*
             * Lọc theo số sao.
             */
            if (soSao.HasValue &&
                soSao.Value >= 1 &&
                soSao.Value <= 5)
            {
                query = query.Where(x =>
                    x.SoSao == soSao.Value);
            }

            /*
             * Lọc theo ngày đánh giá.
             */
            if (tuNgay.HasValue)
            {
                DateTime batDau =
                    tuNgay.Value.Date;

                query = query.Where(x =>
                    x.NgayDanhGia >= batDau);
            }

            if (denNgay.HasValue)
            {
                DateTime ketThuc =
                    denNgay.Value.Date.AddDays(1);

                query = query.Where(x =>
                    x.NgayDanhGia < ketThuc);
            }

            var danhSachDanhGia =
                await query
                    .OrderByDescending(x =>
                        x.NgayDanhGia)
                    .ThenByDescending(x =>
                        x.MaDanhGia)
                    .ToListAsync();

            /*
             * Thống kê toàn bộ đánh giá.
             */
            int tongDanhGia =
                await _context.DanhGias
                    .AsNoTracking()
                    .CountAsync();

            double diemTrungBinh =
                tongDanhGia == 0
                    ? 0
                    : Math.Round(
                        await _context.DanhGias
                            .AsNoTracking()
                            .AverageAsync(x =>
                                (double)x.SoSao),
                        1);

            int danhGiaTot =
                await _context.DanhGias
                    .AsNoTracking()
                    .CountAsync(x =>
                        x.SoSao >= 4);

            int danhGiaCanChuY =
                await _context.DanhGias
                    .AsNoTracking()
                    .CountAsync(x =>
                        x.SoSao <= 2);

            var phanBoSoSao =
                new List<AdminReviewStarItemViewModel>();

            for (int sao = 5;
                 sao >= 1;
                 sao--)
            {
                int soLuong =
                    await _context.DanhGias
                        .AsNoTracking()
                        .CountAsync(x =>
                            x.SoSao == sao);

                double tyLe =
                    tongDanhGia == 0
                        ? 0
                        : Math.Round(
                            soLuong
                            * 100.0
                            / tongDanhGia,
                            1);

                phanBoSoSao.Add(
                    new AdminReviewStarItemViewModel
                    {
                        SoSao = sao,
                        SoLuong = soLuong,
                        TyLe = tyLe
                    });
            }

            var model =
                new AdminReviewViewModel
                {
                    TongDanhGia =
                        tongDanhGia,

                    DiemTrungBinh =
                        diemTrungBinh,

                    DanhGiaTot =
                        danhGiaTot,

                    DanhGiaCanChuY =
                        danhGiaCanChuY,

                    TuKhoa =
                        tuKhoa,

                    SoSao =
                        soSao,

                    TuNgay =
                        tuNgay,

                    DenNgay =
                        denNgay,

                    PhanBoSoSao =
                        phanBoSoSao,

                    DanhSachDanhGia =
                        danhSachDanhGia
                };

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Details(
            int id)
        {
            var model =
                await (
                    from danhGia in _context.DanhGias
                        .AsNoTracking()

                    join lich in _context.DatLichs
                        .AsNoTracking()
                        on danhGia.MaDatLich
                        equals lich.MaDatLich

                    join khachHang in _context.KhachHangs
                        .AsNoTracking()
                        on lich.MaKhachHang
                        equals khachHang.MaKhachHang

                    join benhNhan in _context.BenhNhans
                        .AsNoTracking()
                        on lich.MaBenhNhan
                        equals benhNhan.MaBenhNhan

                    join nguoiChamSocTam
                        in _context.NguoiChamSocs
                            .AsNoTracking()
                        on lich.MaNguoiChamSoc
                        equals nguoiChamSocTam.MaNguoiChamSoc
                        into nhomNguoiChamSoc

                    from nguoiChamSoc
                        in nhomNguoiChamSoc.DefaultIfEmpty()

                    join dichVuTam
                        in _context.DichVus
                            .AsNoTracking()
                        on lich.MaDichVu
                        equals dichVuTam.MaDichVu
                        into nhomDichVu

                    from dichVu
                        in nhomDichVu.DefaultIfEmpty()

                    where danhGia.MaDanhGia == id

                    select new AdminReviewItemViewModel
                    {
                        MaDanhGia =
                            danhGia.MaDanhGia,

                        MaDatLich =
                            danhGia.MaDatLich,

                        SoSao =
                            danhGia.SoSao,

                        NoiDung =
                            danhGia.NoiDung
                            ?? "Khách hàng không để lại nhận xét.",

                        NgayDanhGia =
                            danhGia.NgayDanhGia,

                        TenKhachHang =
                            khachHang.HoTen,

                        TenNguoiChamSoc =
                            nguoiChamSoc == null
                                ? "Chưa phân công"
                                : nguoiChamSoc.HoTen,

                        TenBenhNhan =
                            benhNhan.HoTen,

                        TenDichVu =
                            dichVu == null
                                ? "Chưa cập nhật"
                                : dichVu.TenDichVu,

                        NgayChamSoc =
                            lich.NgayChamSoc
                            ?? DateTime.Today,

                        TrangThaiLich =
                            lich.TrangThai
                            ?? "Chưa xác định"
                    })
                    .FirstOrDefaultAsync();

            if (model == null)
            {
                return NotFound();
            }

            return View(model);
        }
    }
}
