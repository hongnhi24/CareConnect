using CareConnect.Data;
using CareConnect.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CareConnect.Areas.Customer.Controllers
{
    [Area("Customer")]
    [Authorize(Roles = "Khách hàng")]
    public class CareJournalsController : Controller
    {
        private readonly CareConnectDbContext _context;

        public CareJournalsController(
            CareConnectDbContext context)
        {
            _context = context;
        }

        private async Task<int?> LayMaKhachHangDangNhap()
        {
            string tenDangNhap =
                User.Identity?.Name?.Trim()
                ?? string.Empty;

            if (string.IsNullOrWhiteSpace(tenDangNhap))
            {
                return null;
            }

            return await (
                from khachHang
                    in _context.KhachHangs.AsNoTracking()

                join taiKhoan
                    in _context.TaiKhoans.AsNoTracking()
                    on khachHang.MaTaiKhoan
                    equals taiKhoan.MaTaiKhoan

                where taiKhoan.TenDangNhap == tenDangNhap

                select (int?)khachHang.MaKhachHang
            )
            .FirstOrDefaultAsync();
        }

        [HttpGet]
        public async Task<IActionResult> Index(
            string? tuKhoa,
            int? maBenhNhan)
        {
            int? maKhachHang =
                await LayMaKhachHangDangNhap();

            if (!maKhachHang.HasValue)
            {
                return Forbid();
            }

            tuKhoa =
                tuKhoa?.Trim()
                ?? string.Empty;

            var query =
                from nhatKy
                    in _context.NhatKyChamSocs
                        .AsNoTracking()

                join lich
                    in _context.DatLichs.AsNoTracking()
                    on nhatKy.MaDatLich
                    equals lich.MaDatLich

                join benhNhan
                    in _context.BenhNhans.AsNoTracking()
                    on lich.MaBenhNhan
                    equals benhNhan.MaBenhNhan

                join dichVu
                    in _context.DichVus.AsNoTracking()
                    on lich.MaDichVu
                    equals dichVu.MaDichVu

                join nguoiChamSocTam
                    in _context.NguoiChamSocs.AsNoTracking()
                    on lich.MaNguoiChamSoc
                    equals nguoiChamSocTam.MaNguoiChamSoc
                    into nhomNguoiChamSoc

                from nguoiChamSoc
                    in nhomNguoiChamSoc.DefaultIfEmpty()

                where
                    lich.MaKhachHang
                        == maKhachHang.Value

                select new CustomerCareJournalItemViewModel
                {
                    MaNhatKy =
                        nhatKy.MaNhatKy,

                    MaDatLich =
                        lich.MaDatLich,

                    MaBenhNhan =
                        benhNhan.MaBenhNhan,

                    TenBenhNhan =
                        benhNhan.HoTen,

                    TenNguoiChamSoc =
                        nguoiChamSoc == null
                            ? "Chưa phân công"
                            : nguoiChamSoc.HoTen,

                    TenDichVu =
                        dichVu.TenDichVu,

                    NgayChamSoc =
                        lich.NgayChamSoc
                        ?? DateTime.Today,

                    GioBatDau =
                        lich.GioBatDau
                        ?? TimeSpan.Zero,

                    GioKetThuc =
                        lich.GioKetThuc
                        ?? TimeSpan.Zero,

                    TinhTrangSucKhoe =
                        nhatKy.TinhTrangSucKhoe
                        ?? "Chưa cập nhật",

                    HuyetAp =
                        nhatKy.HuyetAp
                        ?? "Chưa cập nhật",

                    NhipTim =
                        nhatKy.NhipTim,

                    NhietDo =
                        nhatKy.NhietDo,

                    CanNang =
                        nhatKy.CanNang,

                    TinhTrangAnUong =
                        nhatKy.TinhTrangAnUong
                        ?? "Chưa cập nhật",

                    ThuocDaUong =
                        nhatKy.ThuocDaUong
                        ?? "Chưa cập nhật",

                    GhiChu =
                        nhatKy.GhiChu
                        ?? "Không có ghi chú.",

                    NgayCapNhat =
                        nhatKy.NgayCapNhat
                };

            if (maBenhNhan.HasValue)
            {
                query = query.Where(x =>
                    x.MaBenhNhan
                        == maBenhNhan.Value);
            }

            if (!string.IsNullOrWhiteSpace(tuKhoa))
            {
                query = query.Where(x =>
                    x.TenBenhNhan.Contains(tuKhoa)
                    ||
                    x.TenNguoiChamSoc.Contains(tuKhoa)
                    ||
                    x.TenDichVu.Contains(tuKhoa)
                    ||
                    x.TinhTrangSucKhoe.Contains(tuKhoa)
                    ||
                    x.TinhTrangAnUong.Contains(tuKhoa)
                    ||
                    x.ThuocDaUong.Contains(tuKhoa));
            }

            var danhSach =
                await query
                    .OrderByDescending(x =>
                        x.NgayChamSoc)
                    .ThenByDescending(x =>
                        x.NgayCapNhat)
                    .ToListAsync();

            var danhSachBenhNhan =
                await _context.BenhNhans
                    .AsNoTracking()
                    .Where(x =>
                        x.MaKhachHang
                            == maKhachHang.Value)
                    .OrderBy(x => x.HoTen)
                    .Select(x =>
                        new CustomerJournalPatientOptionViewModel
                        {
                            MaBenhNhan =
                                x.MaBenhNhan,

                            HoTen =
                                x.HoTen
                        })
                    .ToListAsync();

            var model =
                new CustomerCareJournalListViewModel
                {
                    TongNhatKy =
                        danhSach.Count,

                    TongNguoiThan =
                        danhSachBenhNhan.Count,

                    TuKhoa =
                        tuKhoa,

                    MaBenhNhan =
                        maBenhNhan,

                    DanhSach =
                        danhSach,

                    DanhSachBenhNhan =
                        danhSachBenhNhan
                };

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            int? maKhachHang =
                await LayMaKhachHangDangNhap();

            if (!maKhachHang.HasValue)
            {
                return Forbid();
            }

            var model =
                await (
                    from nhatKy
                        in _context.NhatKyChamSocs
                            .AsNoTracking()

                    join lich
                        in _context.DatLichs.AsNoTracking()
                        on nhatKy.MaDatLich
                        equals lich.MaDatLich

                    join benhNhan
                        in _context.BenhNhans.AsNoTracking()
                        on lich.MaBenhNhan
                        equals benhNhan.MaBenhNhan

                    join dichVu
                        in _context.DichVus.AsNoTracking()
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

                    where
                        nhatKy.MaNhatKy == id
                        &&
                        lich.MaKhachHang
                            == maKhachHang.Value

                    select new CustomerCareJournalDetailsViewModel
                    {
                        MaNhatKy =
                            nhatKy.MaNhatKy,

                        MaDatLich =
                            lich.MaDatLich,

                        MaBenhNhan =
                            benhNhan.MaBenhNhan,

                        TenBenhNhan =
                            benhNhan.HoTen,

                        TenNguoiChamSoc =
                            nguoiChamSoc == null
                                ? "Chưa phân công"
                                : nguoiChamSoc.HoTen,

                        TenDichVu =
                            dichVu.TenDichVu,

                        NgayChamSoc =
                            lich.NgayChamSoc
                            ?? DateTime.Today,

                        GioBatDau =
                            lich.GioBatDau
                            ?? TimeSpan.Zero,

                        GioKetThuc =
                            lich.GioKetThuc
                            ?? TimeSpan.Zero,

                        DiaChiChamSoc =
                            lich.DiaChiChamSoc
                            ?? "Chưa cập nhật",

                        TrangThaiLich =
                            lich.TrangThai
                            ?? "Chưa xác định",

                        TinhTrangSucKhoe =
                            nhatKy.TinhTrangSucKhoe
                            ?? "Chưa cập nhật",

                        HuyetAp =
                            nhatKy.HuyetAp
                            ?? "Chưa cập nhật",

                        NhipTim =
                            nhatKy.NhipTim,

                        NhietDo =
                            nhatKy.NhietDo,

                        CanNang =
                            nhatKy.CanNang,

                        TinhTrangAnUong =
                            nhatKy.TinhTrangAnUong
                            ?? "Chưa cập nhật",

                        ThuocDaUong =
                            nhatKy.ThuocDaUong
                            ?? "Chưa cập nhật",

                        GhiChu =
                            nhatKy.GhiChu
                            ?? "Không có ghi chú.",

                        NgayCapNhat =
                            nhatKy.NgayCapNhat
                    })
                .FirstOrDefaultAsync();

            if (model == null)
            {
                TempData["Error"] =
                    "Không tìm thấy nhật ký chăm sóc.";

                return RedirectToAction(nameof(Index));
            }

            return View(model);
        }
    }
}