using CareConnect.Data;
using CareConnect.Models;
using CareConnect.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CareConnect.Areas.Customer.Controllers
{
    [Area("Customer")]
    [Authorize(Roles = "Khách hàng")]
    public class ReviewsController : Controller
    {
        private readonly CareConnectDbContext _context;

        public ReviewsController(
            CareConnectDbContext context)
        {
            _context = context;
        }

        private async Task<int?> LayMaKhachHangDangNhap()
        {
            string tenDangNhap =
                User.Identity?.Name?.Trim()
                ?? string.Empty;

            if (string.IsNullOrWhiteSpace(
                tenDangNhap))
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

                where
                    taiKhoan.TenDangNhap
                        == tenDangNhap

                select (int?)
                    khachHang.MaKhachHang
            )
            .FirstOrDefaultAsync();
        }

        [HttpGet]
        public async Task<IActionResult> Index(
            string? trangThai)
        {
            int? maKhachHang =
                await LayMaKhachHangDangNhap();

            if (!maKhachHang.HasValue)
            {
                return Forbid();
            }

            trangThai =
                trangThai?.Trim()
                ?? string.Empty;

            var query =
                from lich
                    in _context.DatLichs.AsNoTracking()

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

                join danhGiaTam
                    in _context.DanhGias.AsNoTracking()
                    on lich.MaDatLich
                    equals danhGiaTam.MaDatLich
                    into nhomDanhGia

                from danhGia
                    in nhomDanhGia.DefaultIfEmpty()

                where
                    lich.MaKhachHang
                        == maKhachHang.Value
                    &&
                    lich.TrangThai
                        == "Đã hoàn thành"

                select new CustomerReviewItemViewModel
                {
                    MaDatLich =
                        lich.MaDatLich,

                    MaDanhGia =
                        danhGia == null
                            ? null
                            : danhGia.MaDanhGia,

                    TenBenhNhan =
                        benhNhan.HoTen,

                    TenNguoiChamSoc =
                        nguoiChamSoc == null
                            ? "Chưa cập nhật"
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

                    TrangThaiLich =
                        lich.TrangThai
                        ?? "Chưa xác định",

                    SoSao =
                        danhGia == null
                            ? null
                            : danhGia.SoSao,

                    NoiDung =
                        danhGia == null
                        ||
                        string.IsNullOrWhiteSpace(
                            danhGia.NoiDung)
                            ? string.Empty
                            : danhGia.NoiDung,

                    NgayDanhGia =
                        danhGia == null
                            ? null
                            : danhGia.NgayDanhGia
                };

            if (trangThai == "Chưa đánh giá")
            {
                query = query.Where(x =>
                    !x.MaDanhGia.HasValue);
            }
            else if (trangThai == "Đã đánh giá")
            {
                query = query.Where(x =>
                    x.MaDanhGia.HasValue);
            }

            var danhSach =
                await query
                    .OrderBy(x => x.MaDanhGia.HasValue)
                    .ThenByDescending(x =>
                        x.NgayChamSoc)
                    .ToListAsync();

            var tatCa =
                await (
                    from lich
                        in _context.DatLichs.AsNoTracking()

                    join danhGiaTam
                        in _context.DanhGias.AsNoTracking()
                        on lich.MaDatLich
                        equals danhGiaTam.MaDatLich
                        into nhomDanhGia

                    from danhGia
                        in nhomDanhGia.DefaultIfEmpty()

                    where
                        lich.MaKhachHang
                            == maKhachHang.Value
                        &&
                        lich.TrangThai
                            == "Đã hoàn thành"

                    select new
                    {
                        MaDanhGia =
                            danhGia == null
                                ? (int?)null
                                : danhGia.MaDanhGia,

                        SoSao =
                            danhGia == null
                                ? (int?)null
                                : danhGia.SoSao
                    })
                .ToListAsync();

            var danhSachDaDanhGia =
                tatCa
                    .Where(x =>
                        x.MaDanhGia.HasValue
                        &&
                        x.SoSao.HasValue)
                    .ToList();

            var model =
                new CustomerReviewListViewModel
                {
                    TongLichCoTheDanhGia =
                        tatCa.Count,

                    ChuaDanhGia =
                        tatCa.Count(x =>
                            !x.MaDanhGia.HasValue),

                    DaDanhGia =
                        tatCa.Count(x =>
                            x.MaDanhGia.HasValue),

                    DiemTrungBinh =
                        danhSachDaDanhGia.Count == 0
                            ? 0
                            : danhSachDaDanhGia
                                .Average(x =>
                                    x.SoSao!.Value),

                    TrangThai =
                        trangThai,

                    DanhSach =
                        danhSach
                };

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Create(int id)
        {
            int? maKhachHang =
                await LayMaKhachHangDangNhap();

            if (!maKhachHang.HasValue)
            {
                return Forbid();
            }

            bool daDanhGia =
                await _context.DanhGias
                    .AsNoTracking()
                    .AnyAsync(x =>
                        x.MaDatLich == id);

            if (daDanhGia)
            {
                TempData["Error"] =
                    "Lịch chăm sóc này đã được đánh giá.";

                return RedirectToAction(nameof(Index));
            }

            var model =
                await (
                    from lich
                        in _context.DatLichs.AsNoTracking()

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
                        lich.MaDatLich == id
                        &&
                        lich.MaKhachHang
                            == maKhachHang.Value
                        &&
                        lich.TrangThai
                            == "Đã hoàn thành"

                    select
                        new CustomerReviewCreateViewModel
                        {
                            MaDatLich =
                                lich.MaDatLich,

                            TenBenhNhan =
                                benhNhan.HoTen,

                            TenNguoiChamSoc =
                                nguoiChamSoc == null
                                    ? "Chưa cập nhật"
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
                                ?? TimeSpan.Zero
                        })
                .FirstOrDefaultAsync();

            if (model == null)
            {
                TempData["Error"] =
                    "Chỉ có thể đánh giá lịch đã hoàn thành.";

                return RedirectToAction(nameof(Index));
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            CustomerReviewCreateViewModel model)
        {
            int? maKhachHang =
                await LayMaKhachHangDangNhap();

            if (!maKhachHang.HasValue)
            {
                return Forbid();
            }

            var lich =
                await _context.DatLichs
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x =>
                        x.MaDatLich
                            == model.MaDatLich
                        &&
                        x.MaKhachHang
                            == maKhachHang.Value);

            if (lich == null)
            {
                return NotFound();
            }

            if (lich.TrangThai != "Đã hoàn thành")
            {
                ModelState.AddModelError(
                    string.Empty,
                    "Chỉ có thể đánh giá lịch đã hoàn thành.");
            }

            bool daDanhGia =
                await _context.DanhGias
                    .AsNoTracking()
                    .AnyAsync(x =>
                        x.MaDatLich
                            == model.MaDatLich);

            if (daDanhGia)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "Lịch chăm sóc này đã được đánh giá.");
            }

            if (!ModelState.IsValid)
            {
                await NapThongTinLich(model);

                return View(model);
            }

            var danhGia =
                new DanhGia
                {
                    MaDatLich =
                        model.MaDatLich,

                    SoSao =
                        model.SoSao!.Value,

                    NoiDung =
                        model.NoiDung.Trim(),

                    NgayDanhGia =
                        DateTime.Now
                };

            _context.DanhGias.Add(danhGia);

            await _context.SaveChangesAsync();

            await CapNhatDiemTrungBinhNguoiChamSocTheoLichAsync(
                danhGia.MaDatLich);

            TempData["ReviewSuccess"] = true;

            TempData["ReviewStars"] =
                danhGia.SoSao;

            TempData["ReviewCaregiver"] =
                model.TenNguoiChamSoc;

            return RedirectToAction(
                nameof(Details),
                new
                {
                    id = danhGia.MaDanhGia
                });
        }
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            int? maKhachHang =
                await LayMaKhachHangDangNhap();

            if (!maKhachHang.HasValue)
            {
                return Forbid();
            }

            var model =
                await (
                    from danhGia
                        in _context.DanhGias.AsNoTracking()

                    join lich
                        in _context.DatLichs.AsNoTracking()
                        on danhGia.MaDatLich
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
                        danhGia.MaDanhGia == id
                        &&
                        lich.MaKhachHang
                            == maKhachHang.Value

                    select new CustomerReviewEditViewModel
                    {
                        MaDanhGia =
                            danhGia.MaDanhGia,

                        MaDatLich =
                            lich.MaDatLich,

                        TenBenhNhan =
                            benhNhan.HoTen,

                        TenNguoiChamSoc =
                            nguoiChamSoc == null
                                ? "Chưa cập nhật"
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

                        SoSao =
                            danhGia.SoSao,

                        NoiDung =
                            danhGia.NoiDung
                            ?? string.Empty
                    })
                .FirstOrDefaultAsync();

            if (model == null)
            {
                return NotFound();
            }

            return View(model);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
    CustomerReviewEditViewModel model)
        {
            int? maKhachHang =
                await LayMaKhachHangDangNhap();

            if (!maKhachHang.HasValue)
            {
                return Forbid();
            }

            var danhGia =
                await (
                    from dg in _context.DanhGias

                    join lich in _context.DatLichs
                        on dg.MaDatLich
                        equals lich.MaDatLich

                    where
                        dg.MaDanhGia
                            == model.MaDanhGia
                        &&
                        lich.MaKhachHang
                            == maKhachHang.Value

                    select dg
                )
                .FirstOrDefaultAsync();

            if (danhGia == null)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                await NapThongTinEdit(model);

                return View(model);
            }

            danhGia.SoSao =
                model.SoSao!.Value;

            danhGia.NoiDung =
                model.NoiDung.Trim();

            /*
             * Giữ NgayDanhGia là ngày tạo đánh giá ban đầu.
             * Không đổi ngày này khi chỉnh sửa.
             */

            await _context.SaveChangesAsync();

            await CapNhatDiemTrungBinhNguoiChamSocTheoLichAsync(
                danhGia.MaDatLich);

            TempData["Success"] =
                "Cập nhật đánh giá thành công.";

            return RedirectToAction(
                nameof(Details),
                new
                {
                    id = danhGia.MaDanhGia
                });
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            int? maKhachHang =
                await LayMaKhachHangDangNhap();

            if (!maKhachHang.HasValue)
            {
                return Forbid();
            }

            var danhGia =
                await (
                    from dg in _context.DanhGias

                    join lich in _context.DatLichs
                        on dg.MaDatLich
                        equals lich.MaDatLich

                    where
                        dg.MaDanhGia == id
                        &&
                        lich.MaKhachHang
                            == maKhachHang.Value

                    select dg
                )
                .FirstOrDefaultAsync();

            if (danhGia == null)
            {
                return NotFound();
            }

            int maDatLich =
                danhGia.MaDatLich;

            _context.DanhGias.Remove(danhGia);

            await _context.SaveChangesAsync();

            await CapNhatDiemTrungBinhNguoiChamSocTheoLichAsync(
                maDatLich);

            TempData["Success"] =
                "Đã xóa đánh giá.";

            return RedirectToAction(nameof(Index));
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
                    from danhGia
                        in _context.DanhGias.AsNoTracking()

                    join lich
                        in _context.DatLichs.AsNoTracking()
                        on danhGia.MaDatLich
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
                        danhGia.MaDanhGia == id
                        &&
                        lich.MaKhachHang
                            == maKhachHang.Value

                    select
                        new CustomerReviewDetailsViewModel
                        {
                            MaDanhGia =
                                danhGia.MaDanhGia,

                            MaDatLich =
                                lich.MaDatLich,

                            TenBenhNhan =
                                benhNhan.HoTen,

                            TenNguoiChamSoc =
                                nguoiChamSoc == null
                                    ? "Chưa cập nhật"
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

                            SoSao =
                                danhGia.SoSao,

                            NoiDung =
                                danhGia.NoiDung
                                ?? "Không có nội dung.",

                            NgayDanhGia =
                                danhGia.NgayDanhGia
                        })
                .FirstOrDefaultAsync();

            if (model == null)
            {
                return NotFound();
            }

            return View(model);
        }

        private async Task NapThongTinLich(
            CustomerReviewCreateViewModel model)
        {
            var thongTin =
                await (
                    from lich
                        in _context.DatLichs.AsNoTracking()

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
                        lich.MaDatLich
                            == model.MaDatLich

                    select new
                    {
                        benhNhan.HoTen,
                        dichVu.TenDichVu,
                        TenNguoiChamSoc =
                            nguoiChamSoc == null
                                ? "Chưa cập nhật"
                                : nguoiChamSoc.HoTen,
                        lich.NgayChamSoc,
                        lich.GioBatDau,
                        lich.GioKetThuc
                    })
                .FirstOrDefaultAsync();

            if (thongTin == null)
            {
                return;
            }

            model.TenBenhNhan =
                thongTin.HoTen;

            model.TenDichVu =
                thongTin.TenDichVu;

            model.TenNguoiChamSoc =
                thongTin.TenNguoiChamSoc;

            model.NgayChamSoc =
                thongTin.NgayChamSoc
                ?? DateTime.Today;

            model.GioBatDau =
                thongTin.GioBatDau
                ?? TimeSpan.Zero;

            model.GioKetThuc =
                thongTin.GioKetThuc
                ?? TimeSpan.Zero;
        }
        private async Task NapThongTinEdit(
    CustomerReviewEditViewModel model)
        {
            int? maKhachHang =
                await LayMaKhachHangDangNhap();

            if (!maKhachHang.HasValue)
            {
                return;
            }

            var thongTin =
                await (
                    from danhGia
                        in _context.DanhGias.AsNoTracking()

                    join lich
                        in _context.DatLichs.AsNoTracking()
                        on danhGia.MaDatLich
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
                        danhGia.MaDanhGia
                            == model.MaDanhGia
                        &&
                        lich.MaKhachHang
                            == maKhachHang.Value

                    select new
                    {
                        lich.MaDatLich,

                        TenBenhNhan =
                            benhNhan.HoTen,

                        TenNguoiChamSoc =
                            nguoiChamSoc == null
                                ? "Chưa cập nhật"
                                : nguoiChamSoc.HoTen,

                        TenDichVu =
                            dichVu.TenDichVu,

                        NgayChamSoc =
                            lich.NgayChamSoc,

                        GioBatDau =
                            lich.GioBatDau,

                        GioKetThuc =
                            lich.GioKetThuc
                    })
                .FirstOrDefaultAsync();

            if (thongTin == null)
            {
                return;
            }

            model.MaDatLich =
                thongTin.MaDatLich;

            model.TenBenhNhan =
                thongTin.TenBenhNhan;

            model.TenNguoiChamSoc =
                thongTin.TenNguoiChamSoc;

            model.TenDichVu =
                thongTin.TenDichVu;

            model.NgayChamSoc =
                thongTin.NgayChamSoc
                ?? DateTime.Today;

            model.GioBatDau =
                thongTin.GioBatDau
                ?? TimeSpan.Zero;

            model.GioKetThuc =
                thongTin.GioKetThuc
                ?? TimeSpan.Zero;
        }

        // =====================================================
        // ĐỒNG BỘ ĐIỂM TRUNG BÌNH CỦA NGƯỜI CHĂM SÓC
        // Sau khi khách hàng thêm/sửa/xóa đánh giá.
        // =====================================================
        private async Task
            CapNhatDiemTrungBinhNguoiChamSocTheoLichAsync(
                int maDatLich)
        {
            int? maNguoiChamSoc =
                await _context.DatLichs
                    .AsNoTracking()
                    .Where(x =>
                        x.MaDatLich == maDatLich)
                    .Select(x =>
                        x.MaNguoiChamSoc)
                    .FirstOrDefaultAsync();

            if (!maNguoiChamSoc.HasValue)
            {
                return;
            }

            double? diemTrungBinh =
                await (
                    from danhGia
                        in _context.DanhGias.AsNoTracking()

                    join lich
                        in _context.DatLichs.AsNoTracking()
                        on danhGia.MaDatLich
                        equals lich.MaDatLich

                    where
                        lich.MaNguoiChamSoc
                            == maNguoiChamSoc.Value

                    select (double?)
                        danhGia.SoSao
                )
                .AverageAsync();

            var nguoiChamSoc =
                await _context.NguoiChamSocs
                    .FirstOrDefaultAsync(x =>
                        x.MaNguoiChamSoc
                            == maNguoiChamSoc.Value);

            if (nguoiChamSoc == null)
            {
                return;
            }

            nguoiChamSoc.DanhGia =
                diemTrungBinh.HasValue
                    ? Math.Round(
                        diemTrungBinh.Value,
                        2,
                        MidpointRounding.AwayFromZero)
                    : 0;

            await _context.SaveChangesAsync();
        }

    }
}