using CareConnect.Constants;
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
    public class PaymentsController : Controller
    {
        private readonly CareConnectDbContext _context;
        private static decimal TinhTienDatCoc(
    decimal tongTien,
    decimal tyLeDatCoc)
        {
            return decimal.Round(
                tongTien * tyLeDatCoc / 100m,
                0,
                MidpointRounding.AwayFromZero);
        }

        private static decimal TinhSoTienConLai(
            decimal tongTien,
            decimal daThanhToan)
        {
            return Math.Max(
                0,
                tongTien - daThanhToan);
        }

        private static void DamBaoPhanChiaDoanhThu(
            DatLich lich)
        {
            decimal tongTien =
                lich.TongTien ?? 0;

            if (!lich.TyLePhiNenTang.HasValue)
            {
                lich.TyLePhiNenTang =
                    PaymentPolicy.PlatformFeePercent;
            }

            if (!lich.PhiNenTang.HasValue)
            {
                lich.PhiNenTang =
                    decimal.Round(
                        tongTien
                        * lich.TyLePhiNenTang.Value
                        / 100m,
                        0,
                        MidpointRounding.AwayFromZero);
            }

            if (!lich.ThuNhapNguoiChamSoc.HasValue)
            {
                lich.ThuNhapNguoiChamSoc =
                    tongTien - lich.PhiNenTang.Value;
            }
        }
        public PaymentsController(
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

                join thanhToanTam
                    in _context.ThanhToans.AsNoTracking()
                    on lich.MaDatLich
                    equals thanhToanTam.MaDatLich
                    into nhomThanhToan

                from thanhToan
                    in nhomThanhToan.DefaultIfEmpty()

                where
                    lich.MaKhachHang
                        == maKhachHang.Value

                select
                    new CustomerPaymentItemViewModel
                    {
                        MaDatLich =
                            lich.MaDatLich,

                        MaThanhToan =
                            thanhToan == null
                                ? null
                                : thanhToan.MaThanhToan,

                        MaThanhToanCode =
                            thanhToan == null
                            ||
                            string.IsNullOrWhiteSpace(
                                thanhToan.MaThanhToanCode)
                                ? string.Empty
                                : thanhToan
                                    .MaThanhToanCode,

                        TenBenhNhan =
                            benhNhan.HoTen,

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

                        // Tổng giá trị của cả dịch vụ
                        SoTien =
    lich.TongTien ?? 0,

                        // Tổng số tiền khách đã trả đến hiện tại
                        DaThanhToan =
    thanhToan == null
        ? 0
        : thanhToan.SoTien,

                        LoaiThanhToan =
    thanhToan == null
        ? string.Empty
        : thanhToan.LoaiThanhToan
            ?? string.Empty,

                        NgayDatCoc =
    thanhToan == null
        ? null
        : thanhToan.NgayDatCoc,

                        TrangThaiLich =
    lich.TrangThai
    ?? "Chưa xác định",

                        TrangThaiThanhToan =
                            thanhToan == null
                            ||
                            string.IsNullOrWhiteSpace(
                                thanhToan.TrangThai)
                                ? "Chưa thanh toán"
                                : thanhToan.TrangThai,

                        PhuongThucThanhToan =
                            thanhToan == null
                            ||
                            string.IsNullOrWhiteSpace(
                                thanhToan
                                    .PhuongThucThanhToan)
                                ? "Chưa chọn"
                                : thanhToan
                                    .PhuongThucThanhToan,

                        NgayThanhToan =
                            thanhToan == null
                                ? null
                                : thanhToan.NgayThanhToan
                    };

            if (!string.IsNullOrWhiteSpace(
                trangThai))
            {
                query = query.Where(x =>
                    x.TrangThaiThanhToan
                        == trangThai);
            }

            var danhSach =
                await query
                    .OrderBy(x =>
                        x.TrangThaiThanhToan
                            == "Đã thanh toán")
                    .ThenByDescending(x =>
                        x.NgayChamSoc)
                    .ToListAsync();

            var tatCa =
                await (
                    from lich
                        in _context.DatLichs.AsNoTracking()

                    join thanhToanTam
                        in _context.ThanhToans.AsNoTracking()
                        on lich.MaDatLich
                        equals thanhToanTam.MaDatLich
                        into nhomThanhToan

                    from thanhToan
                        in nhomThanhToan.DefaultIfEmpty()

                    where
                        lich.MaKhachHang
                            == maKhachHang.Value

                    select new
                    {
                        TrangThai =
                            thanhToan == null
                            ||
                            string.IsNullOrWhiteSpace(
                                thanhToan.TrangThai)
                                ? "Chưa thanh toán"
                                : thanhToan.TrangThai,

                                            DaThanhToan =
                            thanhToan == null
                                ? 0
                                : thanhToan.SoTien
                                        })
.ToListAsync();

            var model =
    new CustomerPaymentListViewModel
    {
        TongKhoanThanhToan =
            tatCa.Count,

        ChuaThanhToan =
            tatCa.Count(x =>
                x.TrangThai
                    == "Chưa thanh toán"),

        DaDatCoc =
            tatCa.Count(x =>
                x.TrangThai
                    == "Đã đặt cọc"),

        DaThanhToan =
            tatCa.Count(x =>
                x.TrangThai
                    == "Đã thanh toán"),

        DaHoanTien =
            tatCa.Count(x =>
                x.TrangThai
                    == "Đã hoàn tiền"),

        TongDaThanhToan =
            tatCa
                .Where(x =>
                    x.TrangThai
                        != "Đã hoàn tiền")
                .Sum(x =>
                    x.DaThanhToan),

        TrangThai =
            trangThai,

        DanhSach =
            danhSach
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
                    from lich
                        in _context.DatLichs.AsNoTracking()

                    join khachHang
                        in _context.KhachHangs.AsNoTracking()
                        on lich.MaKhachHang
                        equals khachHang.MaKhachHang

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
                        equals nguoiChamSocTam
                            .MaNguoiChamSoc
                        into nhomNguoiChamSoc

                    from nguoiChamSoc
                        in nhomNguoiChamSoc.DefaultIfEmpty()

                    join thanhToanTam
                        in _context.ThanhToans.AsNoTracking()
                        on lich.MaDatLich
                        equals thanhToanTam.MaDatLich
                        into nhomThanhToan

                    from thanhToan
                        in nhomThanhToan.DefaultIfEmpty()

                    where
                        lich.MaDatLich == id
                        &&
                        lich.MaKhachHang
                            == maKhachHang.Value

                    select
                        new CustomerPaymentDetailsViewModel
                        {
                            MaDatLich =
                                lich.MaDatLich,

                            MaThanhToan =
                                thanhToan == null
                                    ? null
                                    : thanhToan.MaThanhToan,

                            MaThanhToanCode =
                                thanhToan == null
                                ||
                                string.IsNullOrWhiteSpace(
                                    thanhToan
                                        .MaThanhToanCode)
                                    ? string.Empty
                                    : thanhToan
                                        .MaThanhToanCode,

                            TenKhachHang =
                                khachHang.HoTen,

                            TenBenhNhan =
                                benhNhan.HoTen,

                            TenDichVu =
                                dichVu.TenDichVu,

                            TenNguoiChamSoc =
                                nguoiChamSoc == null
                                    ? "Chưa phân công"
                                    : nguoiChamSoc.HoTen,

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

                            // Tổng giá trị dịch vụ
                            SoTien =
                            lich.TongTien ?? 0,

                            // Tổng số tiền khách đã thanh toán
                            DaThanhToan =
                            thanhToan == null
                                ? 0
                                : thanhToan.SoTien,

                            // Tỷ lệ đặt cọc
                            TyLeDatCoc =
                            thanhToan == null
                                ? PaymentPolicy.DepositPercent
                                : thanhToan.TyLeDatCoc,

                            // Loại thanh toán gần nhất
                            LoaiThanhToan =
                            thanhToan == null
                                ? string.Empty
                                : thanhToan.LoaiThanhToan
                                    ?? string.Empty,

                            // Ngày khách đặt cọc
                            NgayDatCoc =
                            thanhToan == null
                                ? null
                                : thanhToan.NgayDatCoc,

                            TrangThaiLich =
                                lich.TrangThai
                                ?? "Chưa xác định",

                            TrangThaiThanhToan =
                                thanhToan == null
                                ||
                                string.IsNullOrWhiteSpace(
                                    thanhToan.TrangThai)
                                    ? "Chưa thanh toán"
                                    : thanhToan.TrangThai,

                            PhuongThucThanhToan =
                                thanhToan == null
                                ||
                                string.IsNullOrWhiteSpace(
                                    thanhToan
                                        .PhuongThucThanhToan)
                                    ? "Chưa chọn"
                                    : thanhToan
                                        .PhuongThucThanhToan,

                            NgayThanhToan =
                                thanhToan == null
                                    ? null
                                    : thanhToan.NgayThanhToan,

                            NgayTaoThanhToan =
                                thanhToan == null
                                    ? null
                                    : thanhToan.NgayTao
                        })
                .FirstOrDefaultAsync();

            if (model == null)
            {
                TempData["Error"] =
                    "Không tìm thấy thông tin thanh toán.";

                return RedirectToAction(nameof(Index));
            }

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Pay(int id)
        {
            int? maKhachHang =
                await LayMaKhachHangDangNhap();

            if (!maKhachHang.HasValue)
            {
                return Forbid();
            }

            var thongTin =
                await (
                    from lich in
                        _context.DatLichs.AsNoTracking()

                    join benhNhan in
                        _context.BenhNhans.AsNoTracking()
                        on lich.MaBenhNhan
                        equals benhNhan.MaBenhNhan

                    join dichVu in
                        _context.DichVus.AsNoTracking()
                        on lich.MaDichVu
                        equals dichVu.MaDichVu

                    where
                        lich.MaDatLich == id
                        &&
                        lich.MaKhachHang
                            == maKhachHang.Value

                    select new
                    {
                        lich.MaDatLich,
                        lich.NgayChamSoc,
                        lich.GioBatDau,
                        lich.GioKetThuc,
                        lich.TongTien,
                        lich.TrangThai,

                        TenBenhNhan =
                            benhNhan.HoTen,

                        dichVu.TenDichVu
                    })
                .FirstOrDefaultAsync();

            if (thongTin == null)
            {
                return NotFound();
            }

            if (thongTin.TrangThai == "Đã hủy")
            {
                TempData["Error"] =
                    "Không thể thanh toán lịch đã hủy.";

                return RedirectToAction(
                    nameof(Details),
                    new { id });
            }

            decimal tongTien =
                thongTin.TongTien ?? 0;

            if (tongTien <= 0)
            {
                TempData["Error"] =
                    "Lịch chăm sóc chưa có số tiền hợp lệ.";

                return RedirectToAction(
                    nameof(Details),
                    new { id });
            }

            var thanhToan =
                await _context.ThanhToans
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x =>
                        x.MaDatLich == id);

            if (thanhToan != null
                &&
                thanhToan.TrangThai
                    == "Đã thanh toán")
            {
                TempData["Error"] =
                    "Lịch này đã được thanh toán đầy đủ.";

                return RedirectToAction(
                    nameof(Details),
                    new { id });
            }

            if (thanhToan != null
                &&
                (
                    thanhToan.TrangThai
                        == "Đã hoàn tiền"
                    ||
                    thanhToan.TrangThai
                        == "Không hoàn phí"
                ))
            {
                TempData["Error"] =
                    "Khoản thanh toán này không thể tiếp tục xử lý.";

                return RedirectToAction(
                    nameof(Details),
                    new { id });
            }

            decimal daThanhToan =
                thanhToan?.SoTien ?? 0;

            bool daDatCoc =
                thanhToan?.TrangThai
                    == "Đã đặt cọc";

            decimal tyLeDatCoc =
                thanhToan?.TyLeDatCoc
                ?? PaymentPolicy.DepositPercent;

            var model =
                new CustomerPaymentConfirmViewModel
                {
                    MaDatLich =
                        thongTin.MaDatLich,

                    TenBenhNhan =
                        thongTin.TenBenhNhan,

                    TenDichVu =
                        thongTin.TenDichVu,

                    NgayChamSoc =
                        thongTin.NgayChamSoc
                        ?? DateTime.Today,

                    GioBatDau =
                        thongTin.GioBatDau
                        ?? TimeSpan.Zero,

                    GioKetThuc =
                        thongTin.GioKetThuc
                        ?? TimeSpan.Zero,

                    SoTien =
                        tongTien,

                    DaThanhToan =
                        daThanhToan,

                    TyLeDatCoc =
                        tyLeDatCoc,

                    SoTienDatCoc =
                        TinhTienDatCoc(
                            tongTien,
                            tyLeDatCoc),

                    SoTienConLai =
                        TinhSoTienConLai(
                            tongTien,
                            daThanhToan),

                    DaDatCoc =
                        daDatCoc,

                    LoaiThanhToan =
                        daDatCoc
                            ? "Thanh toán phần còn lại"
                            : string.Empty
                };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Pay(
    CustomerPaymentConfirmViewModel model)
        {
            int? maKhachHang =
                await LayMaKhachHangDangNhap();

            if (!maKhachHang.HasValue)
            {
                return Forbid();
            }

            var lich =
                await _context.DatLichs
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

            if (lich.TrangThai == "Đã hủy")
            {
                ModelState.AddModelError(
                    string.Empty,
                    "Không thể thanh toán lịch đã hủy.");
            }

            decimal tongTien =
                lich.TongTien ?? 0;

            if (tongTien <= 0)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "Lịch chăm sóc chưa có số tiền hợp lệ.");
            }

            var thanhToan =
                await _context.ThanhToans
                    .FirstOrDefaultAsync(x =>
                        x.MaDatLich
                            == model.MaDatLich);

            if (thanhToan != null
                &&
                thanhToan.TrangThai
                    == "Đã thanh toán")
            {
                ModelState.AddModelError(
                    string.Empty,
                    "Lịch này đã được thanh toán đầy đủ.");
            }

            if (thanhToan != null
                &&
                (
                    thanhToan.TrangThai
                        == "Đã hoàn tiền"
                    ||
                    thanhToan.TrangThai
                        == "Không hoàn phí"
                ))
            {
                ModelState.AddModelError(
                    string.Empty,
                    "Khoản thanh toán này không thể tiếp tục xử lý.");
            }

            bool daDatCoc =
                thanhToan?.TrangThai
                    == "Đã đặt cọc";

            // Nếu đã cọc thì hệ thống tự động ép
            // sang thanh toán phần còn lại.
            if (daDatCoc)
            {
                model.LoaiThanhToan =
                    "Thanh toán phần còn lại";
            }
            else
            {
                bool loaiHopLe =
                    model.LoaiThanhToan
                        == "Đặt cọc"
                    ||
                    model.LoaiThanhToan
                        == "Thanh toán toàn bộ";

                if (!loaiHopLe)
                {
                    ModelState.AddModelError(
                        nameof(model.LoaiThanhToan),
                        "Vui lòng chọn đặt cọc hoặc thanh toán toàn bộ.");
                }
            }

            if (!ModelState.IsValid)
            {
                await NapThongTinThanhToan(model);

                return View(model);
            }

            DateTime bayGio =
                DateTime.Now;

            decimal soTienLanNay = 0;

            string loaiThanhToan =
                model.LoaiThanhToan.Trim();

            // =====================================================
            // TRƯỜNG HỢP 1:
            // ĐÃ CỌC -> THANH TOÁN PHẦN CÒN LẠI
            // =====================================================

            if (daDatCoc && thanhToan != null)
            {
                soTienLanNay =
                    TinhSoTienConLai(
                        tongTien,
                        thanhToan.SoTien);

                if (soTienLanNay <= 0)
                {
                    ModelState.AddModelError(
                        string.Empty,
                        "Khoản thanh toán không còn số dư.");

                    await NapThongTinThanhToan(model);

                    return View(model);
                }

                // SoTien lưu TỔNG tiền khách đã trả.
                thanhToan.SoTien =
                    tongTien;

                thanhToan.LoaiThanhToan =
                    "Thanh toán phần còn lại";

                thanhToan.PhuongThucThanhToan =
                    model.PhuongThucThanhToan.Trim();

                thanhToan.TrangThai =
                    "Đã thanh toán";

                thanhToan.NgayThanhToan =
                    bayGio;
            }

            // =====================================================
            // TRƯỜNG HỢP 2:
            // KHÁCH CHỌN ĐẶT CỌC
            // =====================================================

            else if (loaiThanhToan == "Đặt cọc")
            {
                decimal tienDatCoc =
                    TinhTienDatCoc(
                        tongTien,
                        PaymentPolicy.DepositPercent);

                soTienLanNay =
                    tienDatCoc;

                if (thanhToan == null)
                {
                    thanhToan =
                        new ThanhToan
                        {
                            MaDatLich =
                                model.MaDatLich,

                            SoTien =
                                tienDatCoc,

                            TyLeDatCoc =
                                PaymentPolicy.DepositPercent,

                            LoaiThanhToan =
                                "Đặt cọc",

                            PhuongThucThanhToan =
                                model
                                    .PhuongThucThanhToan
                                    .Trim(),

                            TrangThai =
                                "Đã đặt cọc",

                            NgayDatCoc =
                                bayGio,

                            NgayThanhToan =
                                null,

                            NgayTao =
                                bayGio
                        };

                    _context.ThanhToans.Add(
                        thanhToan);
                }
                else
                {
                    thanhToan.SoTien =
                        tienDatCoc;

                    thanhToan.TyLeDatCoc =
                        PaymentPolicy.DepositPercent;

                    thanhToan.LoaiThanhToan =
                        "Đặt cọc";

                    thanhToan.PhuongThucThanhToan =
                        model
                            .PhuongThucThanhToan
                            .Trim();

                    thanhToan.TrangThai =
                        "Đã đặt cọc";

                    thanhToan.NgayDatCoc =
                        bayGio;

                    thanhToan.NgayThanhToan =
                        null;
                }
            }

            // =====================================================
            // TRƯỜNG HỢP 3:
            // KHÁCH THANH TOÁN LUÔN 100%
            // =====================================================

            else
            {
                soTienLanNay =
                    tongTien;

                if (thanhToan == null)
                {
                    thanhToan =
                        new ThanhToan
                        {
                            MaDatLich =
                                model.MaDatLich,

                            SoTien =
                                tongTien,

                            TyLeDatCoc =
                                PaymentPolicy.DepositPercent,

                            LoaiThanhToan =
                                "Thanh toán toàn bộ",

                            PhuongThucThanhToan =
                                model
                                    .PhuongThucThanhToan
                                    .Trim(),

                            TrangThai =
                                "Đã thanh toán",

                            NgayThanhToan =
                                bayGio,

                            NgayTao =
                                bayGio
                        };

                    _context.ThanhToans.Add(
                        thanhToan);
                }
                else
                {
                    thanhToan.SoTien =
                        tongTien;

                    thanhToan.TyLeDatCoc =
                        PaymentPolicy.DepositPercent;

                    thanhToan.LoaiThanhToan =
                        "Thanh toán toàn bộ";

                    thanhToan.PhuongThucThanhToan =
                        model
                            .PhuongThucThanhToan
                            .Trim();

                    thanhToan.TrangThai =
                        "Đã thanh toán";

                    thanhToan.NgayThanhToan =
                        bayGio;
                }
            }

            // Snapshot phí nền tảng và thu nhập caregiver.
            DamBaoPhanChiaDoanhThu(lich);

            try
            {
                await _context.SaveChangesAsync();

                await _context
                    .Entry(thanhToan!)
                    .ReloadAsync();
            }
            catch (DbUpdateException ex)
            {
                string loiChiTiet =
                    ex.InnerException?.Message
                    ?? ex.Message;

                ModelState.AddModelError(
                    string.Empty,
                    $"Không thể lưu thanh toán: {loiChiTiet}");

                await NapThongTinThanhToan(model);

                return View(model);
            }

            TempData["PaymentSuccess"] =
                true;

            TempData["PaymentSuccessMessage"] =
                loaiThanhToan == "Đặt cọc"
                    ? $"Đã ghi nhận đặt cọc {PaymentPolicy.DepositPercent:0}% cho lịch chăm sóc."
                    : daDatCoc
                        ? "Đã thanh toán thành công số tiền còn lại."
                        : "Đã thanh toán đầy đủ chi phí dịch vụ.";

            TempData["PaymentCode"] =
                thanhToan!.MaThanhToanCode
                ?? $"TT{thanhToan.MaThanhToan:D4}";

            // Chỉ hiển thị số tiền của GIAO DỊCH LẦN NÀY.
            TempData["PaymentAmount"] =
                soTienLanNay.ToString("#,##0");

            TempData["PaymentMethod"] =
                thanhToan.PhuongThucThanhToan
                ?? "Chưa xác định";

            return RedirectToAction(
                nameof(Details),
                new
                {
                    id = model.MaDatLich
                });
        }

        private async Task NapThongTinThanhToan(
     CustomerPaymentConfirmViewModel model)
        {
            var thongTin =
                await (
                    from lich in
                        _context.DatLichs.AsNoTracking()

                    join benhNhan in
                        _context.BenhNhans.AsNoTracking()
                        on lich.MaBenhNhan
                        equals benhNhan.MaBenhNhan

                    join dichVu in
                        _context.DichVus.AsNoTracking()
                        on lich.MaDichVu
                        equals dichVu.MaDichVu

                    where
                        lich.MaDatLich
                            == model.MaDatLich

                    select new
                    {
                        benhNhan.HoTen,
                        dichVu.TenDichVu,
                        lich.NgayChamSoc,
                        lich.GioBatDau,
                        lich.GioKetThuc,
                        lich.TongTien
                    })
                .FirstOrDefaultAsync();

            if (thongTin == null)
            {
                return;
            }

            var thanhToan =
                await _context.ThanhToans
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x =>
                        x.MaDatLich
                            == model.MaDatLich);

            decimal tongTien =
                thongTin.TongTien ?? 0;

            decimal daThanhToan =
                thanhToan?.SoTien ?? 0;

            decimal tyLeDatCoc =
                thanhToan?.TyLeDatCoc
                ?? PaymentPolicy.DepositPercent;

            model.TenBenhNhan =
                thongTin.HoTen;

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

            model.SoTien =
                tongTien;

            model.DaThanhToan =
                daThanhToan;

            model.TyLeDatCoc =
                tyLeDatCoc;

            model.SoTienDatCoc =
                TinhTienDatCoc(
                    tongTien,
                    tyLeDatCoc);

            model.SoTienConLai =
                TinhSoTienConLai(
                    tongTien,
                    daThanhToan);

            model.DaDatCoc =
                thanhToan?.TrangThai
                    == "Đã đặt cọc";

            if (model.DaDatCoc)
            {
                model.LoaiThanhToan =
                    "Thanh toán phần còn lại";
            }
        }
    }
}