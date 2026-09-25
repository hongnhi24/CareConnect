using System.Data;
using System.Security.Claims;

using CareConnect.Constants;
using CareConnect.Data;
using CareConnect.Models;
using CareConnect.Services;
using CareConnect.ViewModels;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;


namespace CareConnect.Areas.Customer.Controllers
{
    [Area("Customer")]
    [Authorize(Roles = "Khách hàng")]
    public class BookingsController : Controller
    {
        private readonly CareConnectDbContext _context;

        private readonly ICaregiverMatchingService
            _matchingService;


        // =====================================================
        // CONSTRUCTOR
        // =====================================================

        public BookingsController(
            CareConnectDbContext context,
            ICaregiverMatchingService matchingService)
        {
            _context =
                context;

            _matchingService =
                matchingService;
        }



        // =====================================================
        // CÁC LÝ DO HỦY LỊCH HỢP LỆ
        // =====================================================

        private static readonly HashSet<string>
            LyDoHuyHopLe =
                new(StringComparer.OrdinalIgnoreCase)
                {
                    "Thay đổi kế hoạch cá nhân",
                    "Không còn nhu cầu chăm sóc",
                    "Thay đổi ngày hoặc giờ chăm sóc",
                    "Đã tìm được phương án chăm sóc khác",
                    "Người được chăm sóc nhập viện",
                    "Không hài lòng với việc phân công",
                    "Lý do khác"
                };



        // =====================================================
        // MODEL NỘI BỘ DÙNG TÍNH HOÀN TIỀN
        // =====================================================

        private sealed class KetQuaHoanTien
        {
            public decimal TyLe { get; set; }

            public decimal SoTien { get; set; }

            public bool HuyGiuaChung { get; set; }

            public string MoTa { get; set; }
                = string.Empty;
        }



        // =====================================================
        // LẤY MÃ TÀI KHOẢN ĐANG ĐĂNG NHẬP
        // =====================================================

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



        // =====================================================
        // LẤY KHÁCH HÀNG ĐANG ĐĂNG NHẬP
        // =====================================================

        private async Task<KhachHang?>
            LayKhachHangDangNhap()
        {
            int? maTaiKhoan =
                LayMaTaiKhoan();


            if (!maTaiKhoan.HasValue)
            {
                return null;
            }


            return await _context.KhachHangs
                .AsNoTracking()
                .FirstOrDefaultAsync(x =>
                    x.MaTaiKhoan
                        == maTaiKhoan.Value);
        }



        // =====================================================
        // LẤY MÃ KHÁCH HÀNG ĐANG ĐĂNG NHẬP
        // =====================================================

        private async Task<int?>
            LayMaKhachHangDangNhap()
        {
            int? maTaiKhoan =
                LayMaTaiKhoan();


            if (!maTaiKhoan.HasValue)
            {
                return null;
            }


            return await _context.KhachHangs
                .AsNoTracking()
                .Where(x =>
                    x.MaTaiKhoan
                        == maTaiKhoan.Value)
                .Select(x =>
                    (int?)x.MaKhachHang)
                .FirstOrDefaultAsync();
        }



        // =====================================================
        // KIỂM TRA TRẠNG THÁI KHÁCH ĐƯỢC PHÉP HỦY
        // =====================================================

        private static bool CoTheKhachHangHuy(
            string? trangThai)
        {
            return trangThai
                    == BookingStatus.ChoXacNhan
                ||
                trangThai
                    == BookingStatus.DaXacNhan
                ||
                trangThai
                    == BookingStatus.DangThucHien;
        }



        // =====================================================
        // TÍNH CHÍNH SÁCH HOÀN TIỀN
        // =====================================================
        private static KetQuaHoanTien
            TinhChinhSachHoanTien(
                string trangThai,
                DateTime ngayChamSoc,
                TimeSpan gioBatDau,
                decimal soTienDaThanhToan)
        {
            /*
             * Chỉ hoàn trên số tiền khách
             * THỰC TẾ đã thanh toán.
             */
            if (soTienDaThanhToan < 0)
            {
                soTienDaThanhToan = 0;
            }


            DateTime thoiDiemBatDau =
                ngayChamSoc.Date
                    .Add(gioBatDau);


            // =====================================================
            // CHỜ XÁC NHẬN
            // → HOÀN 100% TIỀN ĐÃ THANH TOÁN
            // =====================================================

            if (trangThai
                == BookingStatus.ChoXacNhan)
            {
                return new KetQuaHoanTien
                {
                    TyLe =
                        100m,

                    SoTien =
                        soTienDaThanhToan,

                    HuyGiuaChung =
                        false,

                    MoTa =
                        "Lịch chưa được người chăm sóc xác nhận. "
                        + "Khách hàng được hoàn 100% "
                        + "số tiền đã thanh toán."
                };
            }


            // =====================================================
            // ĐANG THỰC HIỆN
            // → DỪNG GIỮA CHỪNG
            // → KHÔNG HOÀN
            // =====================================================

            if (trangThai
                == BookingStatus.DangThucHien)
            {
                return new KetQuaHoanTien
                {
                    TyLe =
                        0m,

                    SoTien =
                        0m,

                    HuyGiuaChung =
                        true,

                    MoTa =
                        "Dịch vụ đã bắt đầu. "
                        + "Khách hàng có thể yêu cầu dừng "
                        + "nhưng không được hoàn phí."
                };
            }


            TimeSpan thoiGianConLai =
                thoiDiemBatDau
                - DateTime.Now;


            // =====================================================
            // >= 24 GIỜ
            // → 100%
            // =====================================================

            if (thoiGianConLai.TotalHours >= 24)
            {
                return new KetQuaHoanTien
                {
                    TyLe =
                        100m,

                    SoTien =
                        soTienDaThanhToan,

                    HuyGiuaChung =
                        false,

                    MoTa =
                        "Hủy trước giờ chăm sóc ít nhất 24 giờ. "
                        + "Hoàn 100% số tiền đã thanh toán."
                };
            }


            // =====================================================
            // >= 6 GIỜ VÀ < 24 GIỜ
            // → 50%
            // =====================================================

            if (thoiGianConLai.TotalHours >= 6)
            {
                decimal soTienHoan =
                    decimal.Round(
                        soTienDaThanhToan
                        * 0.5m,
                        0,
                        MidpointRounding.AwayFromZero);


                return new KetQuaHoanTien
                {
                    TyLe =
                        50m,

                    SoTien =
                        soTienHoan,

                    HuyGiuaChung =
                        false,

                    MoTa =
                        "Hủy trước giờ chăm sóc từ 6 đến dưới "
                        + "24 giờ. Hoàn 50% số tiền đã thanh toán."
                };
            }


            // =====================================================
            // < 6 GIỜ
            // → KHÔNG HOÀN
            // =====================================================

            return new KetQuaHoanTien
            {
                TyLe =
                    0m,

                SoTien =
                    0m,

                HuyGiuaChung =
                    false,

                MoTa =
                    "Hủy trước giờ chăm sóc dưới 6 giờ. "
                    + "Không hoàn phí."
            };
        }
        private async Task<decimal>
    LaySoTienDaThanhToan(
        int maDatLich)
        {
            /*
             * Chỉ tính những khoản tiền
             * hệ thống đã ghi nhận là đã thu.
             */

            decimal tongDaThanhToan =
                await _context.ThanhToans
                    .AsNoTracking()
                    .Where(x =>
                        x.MaDatLich == maDatLich
                        &&
                        (
                            x.TrangThai == "Đã đặt cọc"
                            ||
                            x.TrangThai == "Đã thanh toán"
                        ))
                    .SumAsync(x =>
                        (decimal?)x.SoTien)
                ?? 0m;


            return Math.Max(
                0m,
                tongDaThanhToan);
        }


        // =====================================================
        // NẠP BỆNH NHÂN + DỊCH VỤ CHO FORM ĐẶT LỊCH
        // =====================================================

        private async Task NapDuLieuFormDatLich(
            CustomerBookingCreateViewModel model,
            int maKhachHang)
        {
            model.DanhSachBenhNhan =
                await _context.BenhNhans
                    .AsNoTracking()
                    .Where(x =>
                        x.MaKhachHang
                            == maKhachHang
                        &&
                        x.TrangThaiHoatDong)
                    .OrderBy(x =>
                        x.HoTen)
                    .Select(x =>
                        new SelectListItem
                        {
                            Value =
                                x.MaBenhNhan
                                    .ToString(),

                            Text =
                                x.HoTen
                        })
                    .ToListAsync();


            model.DanhSachDichVu =
                await _context.DichVus
                    .AsNoTracking()
                    .Where(x =>
                        x.TrangThai)
                    .OrderBy(x =>
                        x.TenDichVu)
                    .Select(x =>
                        new CustomerServiceOptionViewModel
                        {
                            MaDichVu =
                                x.MaDichVu,

                            MaDichVuCode =
                                x.MaDichVuCode,

                            TenDichVu =
                                x.TenDichVu,

                            MoTa =
                                x.MoTa
                                ?? string.Empty,

                            Gia =
                                x.Gia,

                            ThoiLuong =
                                x.ThoiLuong
                        })
                    .ToListAsync();
        }



        // =====================================================
        // TRANG XÁC NHẬN HỦY LỊCH
        // =====================================================

        [HttpGet]
        public async Task<IActionResult> HuyLich(
            int id)
        {
            int? maTaiKhoan =
                LayMaTaiKhoan();


            if (!maTaiKhoan.HasValue)
            {
                return Unauthorized();
            }


            var khachHang =
                await _context.KhachHangs
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x =>
                        x.MaTaiKhoan
                            == maTaiKhoan.Value);


            if (khachHang == null)
            {
                return NotFound(
                    "Không tìm thấy hồ sơ khách hàng.");
            }


            var lich =
                await (
                    from datLich
                        in _context.DatLichs
                            .AsNoTracking()

                    join dichVu
                        in _context.DichVus
                            .AsNoTracking()

                        on datLich.MaDichVu
                        equals dichVu.MaDichVu

                    join benhNhan
                        in _context.BenhNhans
                            .AsNoTracking()

                        on datLich.MaBenhNhan
                        equals benhNhan.MaBenhNhan

                    where datLich.MaDatLich
                            == id
                        &&
                        datLich.MaKhachHang
                            == khachHang.MaKhachHang

                    select new
                    {
                        DatLich =
                            datLich,

                        TenDichVu =
                            dichVu.TenDichVu,

                        TenBenhNhan =
                            benhNhan.HoTen
                    }
                )
                .FirstOrDefaultAsync();


            if (lich == null)
            {
                return NotFound(
                    "Không tìm thấy lịch đặt.");
            }


            // -------------------------------------------------
            // CHỈ 3 TRẠNG THÁI ĐƯỢC PHÉP HỦY
            // -------------------------------------------------

            if (!CoTheKhachHangHuy(
                    lich.DatLich.TrangThai))
            {
                TempData["Error"] =
                    lich.DatLich.TrangThai
                        == BookingStatus.DaHoanThanh

                        ? "Lịch đã hoàn thành nên không thể hủy."

                        : lich.DatLich.TrangThai
                            == BookingStatus.DaHuy

                            ? "Lịch này đã được hủy trước đó."

                            : "Trạng thái hiện tại của lịch "
                              + "không cho phép hủy.";


                return RedirectToAction(
                    nameof(Index));
            }


            // -------------------------------------------------
            // ĐÃ CÓ LỊCH SỬ HỦY
            // -------------------------------------------------

            bool daHuy =
                await _context.HuyLichs
                    .AsNoTracking()
                    .AnyAsync(x =>
                        x.MaDatLich
                            == lich.DatLich.MaDatLich);


            if (daHuy)
            {
                TempData["Error"] =
                    "Lịch này đã có thông tin hủy trước đó.";


                return RedirectToAction(
                    nameof(Index));
            }


            // -------------------------------------------------
            // VALIDATION NGÀY GIỜ
            // -------------------------------------------------

            if (!lich.DatLich.NgayChamSoc.HasValue
                ||
                !lich.DatLich.GioBatDau.HasValue
                ||
                !lich.DatLich.GioKetThuc.HasValue)
            {
                TempData["Error"] =
                    "Lịch không có đầy đủ "
                    + "thông tin ngày giờ.";


                return RedirectToAction(
                    nameof(Index));
            }


            if (lich.DatLich.GioKetThuc.Value
                <= lich.DatLich.GioBatDau.Value)
            {
                TempData["Error"] =
                    "Khung giờ chăm sóc không hợp lệ.";


                return RedirectToAction(
                    nameof(Index));
            }


            decimal tongTien =
     lich.DatLich.TongTien
     ?? 0m;

            decimal soTienDaThanhToan =
                await LaySoTienDaThanhToan(
                    lich.DatLich.MaDatLich);

            var chinhSach =
                TinhChinhSachHoanTien(
                    lich.DatLich.TrangThai
                        ?? string.Empty,

                    lich.DatLich.NgayChamSoc.Value,

                    lich.DatLich.GioBatDau.Value,

                    soTienDaThanhToan);


            var model =
                new CustomerCancelBookingViewModel
                {
                    MaDatLich =
                        lich.DatLich.MaDatLich,

                    TenDichVu =
                        lich.TenDichVu,

                    TenBenhNhan =
                        lich.TenBenhNhan,

                    NgayChamSoc =
                        lich.DatLich.NgayChamSoc
                            .Value,

                    GioBatDau =
                        lich.DatLich.GioBatDau
                            .Value,

                    GioKetThuc =
                        lich.DatLich.GioKetThuc
                            .Value,

                    TrangThai =
                        lich.DatLich.TrangThai
                        ?? string.Empty,

                    TongTien =
                        tongTien,

                    TyLeHoanDuKien =
                        chinhSach.TyLe,

                    SoTienHoanDuKien =
                        chinhSach.SoTien,

                    HuyGiuaChung =
                        chinhSach.HuyGiuaChung,

                    ChinhSachApDung =
                        chinhSach.MoTa
                };


            return View(model);
        }



        // =====================================================
        // XÁC NHẬN HỦY LỊCH
        // =====================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> HuyLich(
            CustomerCancelBookingViewModel model)
        {
            int? maTaiKhoan =
                LayMaTaiKhoan();


            if (!maTaiKhoan.HasValue)
            {
                return Unauthorized();
            }


            string lyDo =
                model.LyDo?.Trim()
                ?? string.Empty;


            string? ghiChu =
                string.IsNullOrWhiteSpace(
                    model.GhiChu)

                    ? null

                    : model.GhiChu.Trim();


            // -------------------------------------------------
            // VALIDATION LÝ DO
            // -------------------------------------------------

            if (!LyDoHuyHopLe.Contains(
                    lyDo))
            {
                ModelState.AddModelError(
                    nameof(model.LyDo),
                    "Lý do hủy lịch không hợp lệ.");
            }


            if (string.Equals(
                    lyDo,
                    "Lý do khác",
                    StringComparison.OrdinalIgnoreCase)
                &&
                string.IsNullOrWhiteSpace(
                    ghiChu))
            {
                ModelState.AddModelError(
                    nameof(model.GhiChu),
                    "Vui lòng nhập ghi chú "
                    + "khi chọn lý do khác.");
            }


            if (!ModelState.IsValid)
            {
                TempData["Error"] =
                    ModelState.Values
                        .SelectMany(x =>
                            x.Errors)
                        .Select(x =>
                            x.ErrorMessage)
                        .FirstOrDefault()
                    ??
                    "Thông tin hủy lịch không hợp lệ.";


                return RedirectToAction(
                    nameof(HuyLich),
                    new
                    {
                        id =
                            model.MaDatLich
                    });
            }


            // -------------------------------------------------
            // KHÁCH HÀNG
            // -------------------------------------------------

            var khachHang =
                await _context.KhachHangs
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x =>
                        x.MaTaiKhoan
                            == maTaiKhoan.Value);


            if (khachHang == null)
            {
                return NotFound(
                    "Không tìm thấy hồ sơ khách hàng.");
            }


            // -------------------------------------------------
            // BOOKING PHẢI THUỘC KHÁCH ĐANG LOGIN
            // -------------------------------------------------

            var datLich =
                await _context.DatLichs
                    .FirstOrDefaultAsync(x =>
                        x.MaDatLich
                            == model.MaDatLich
                        &&
                        x.MaKhachHang
                            == khachHang.MaKhachHang);


            if (datLich == null)
            {
                return Forbid();
            }


            // -------------------------------------------------
            // RACE CONDITION
            // -------------------------------------------------

            if (!CoTheKhachHangHuy(
                    datLich.TrangThai))
            {
                TempData["Error"] =
                    datLich.TrangThai
                        == BookingStatus.DaHoanThanh

                        ? "Lịch vừa được hoàn thành "
                          + "và không thể hủy."

                        : datLich.TrangThai
                            == BookingStatus.DaHuy

                            ? "Lịch này đã được hủy trước đó."

                            : "Trạng thái của lịch vừa thay đổi. "
                              + "Không thể thực hiện yêu cầu hủy.";


                return RedirectToAction(
                    nameof(Index));
            }


            // -------------------------------------------------
            // VALIDATION NGÀY GIỜ
            // -------------------------------------------------

            if (!datLich.NgayChamSoc.HasValue
                ||
                !datLich.GioBatDau.HasValue
                ||
                !datLich.GioKetThuc.HasValue)
            {
                TempData["Error"] =
                    "Dữ liệu ngày giờ của lịch không hợp lệ.";


                return RedirectToAction(
                    nameof(Index));
            }


            if (datLich.GioKetThuc.Value
                <= datLich.GioBatDau.Value)
            {
                TempData["Error"] =
                    "Khung giờ chăm sóc không hợp lệ.";


                return RedirectToAction(
                    nameof(Index));
            }


            // -------------------------------------------------
            // CHỐNG HỦY HAI LẦN
            // -------------------------------------------------

            bool daCoYeuCauHuy =
                await _context.HuyLichs
                    .AsNoTracking()
                    .AnyAsync(x =>
                        x.MaDatLich
                            == datLich.MaDatLich);


            if (daCoYeuCauHuy)
            {
                TempData["Error"] =
                    "Lịch này đã có thông tin hủy trước đó.";


                return RedirectToAction(
                    nameof(Index));
            }


            // -------------------------------------------------
            // SERVER TÍNH LẠI CHÍNH SÁCH
            // -------------------------------------------------

            decimal tongTien =
    datLich.TongTien
    ?? 0m;


            decimal soTienDaThanhToan =
                await LaySoTienDaThanhToan(
                    datLich.MaDatLich);


            var chinhSach =
                TinhChinhSachHoanTien(
                    datLich.TrangThai
                        ?? string.Empty,

                    datLich.NgayChamSoc.Value,

                    datLich.GioBatDau.Value,

                    soTienDaThanhToan);


            string trangThaiLucHuy =
                datLich.TrangThai
                ?? string.Empty;


            DateTime thoiDiemHuy =
                DateTime.Now;


            // -------------------------------------------------
            // TRANSACTION
            // -------------------------------------------------

            await using var transaction =
                await _context.Database
                    .BeginTransactionAsync();


            try
            {
                // =============================================
                // 1. LƯU BẢNG HUYLICH
                // =============================================

                var huyLich =
                    new HuyLich
                    {
                        MaDatLich =
                            datLich.MaDatLich,

                        MaKhachHang =
                            khachHang.MaKhachHang,

                        LyDo =
                            lyDo,

                        GhiChu =
                            ghiChu,

                        ThoiDiemHuy =
                            thoiDiemHuy,

                        TrangThaiLucHuy =
                            trangThaiLucHuy,

                        TongTien =
                            tongTien,

                        TyLeHoan =
                            chinhSach.TyLe,

                        SoTienHoan =
                            chinhSach.SoTien,
                        SoTienDaThanhToan =
                            soTienDaThanhToan,

                        HuyGiuaChung =
                            chinhSach.HuyGiuaChung,

                        TrangThaiHoanTien =
                            chinhSach.SoTien > 0
                                ? "Chờ hoàn tiền"
                                : "Không hoàn"
                    };


                _context.HuyLichs.Add(
                    huyLich);


                // =============================================
                // 2. CẬP NHẬT BOOKING
                // =============================================

                datLich.TrangThai =
                    BookingStatus.DaHuy;


                /*
                 * Giữ lại 3 field cũ trong DatLich
                 * để tương thích với những phần khác
                 * của project đang đọc trực tiếp DatLich.
                 */
                datLich.LyDoHuy =
                    lyDo;


                datLich.NguoiHuy =
                    "Khách hàng";


                datLich.NgayHuy =
                    thoiDiemHuy;


                // =============================================
                // 3. CẬP NHẬT TRẠNG THÁI THANH TOÁN
                // =============================================

                var danhSachThanhToan =
                    await _context.ThanhToans
                        .Where(x =>
                            x.MaDatLich
                                == datLich.MaDatLich)
                        .ToListAsync();


                foreach (
                    var thanhToan
                    in danhSachThanhToan)
                {
                    /*
                     * Chỉ những giao dịch đã thu tiền
                     * mới cần đổi trạng thái.
                     */

                    if (thanhToan.TrangThai == "Đã đặt cọc"
                        ||
                        thanhToan.TrangThai == "Đã thanh toán")
                    {
                        thanhToan.TrangThai =
                            chinhSach.SoTien > 0
                                ? "Chờ hoàn tiền"
                                : "Không hoàn";
                    }
                }


                // =============================================
                // 4. THÔNG BÁO CHO CAREGIVER
                // =============================================

                if (datLich.MaNguoiChamSoc.HasValue)
                {
                    var nguoiChamSoc =
                        await _context.NguoiChamSocs
                            .AsNoTracking()
                            .FirstOrDefaultAsync(x =>
                                x.MaNguoiChamSoc
                                    == datLich
                                        .MaNguoiChamSoc
                                        .Value);


                    if (nguoiChamSoc != null
                        &&
                        nguoiChamSoc
                            .MaTaiKhoan
                            .HasValue)
                    {
                        string noiDungCaregiver;


                        if (chinhSach.HuyGiuaChung)
                        {
                            noiDungCaregiver =
                                $"Khách hàng đã yêu cầu dừng "
                                + $"lịch #DL{datLich.MaDatLich} "
                                + "trong khi dịch vụ "
                                + "đang được thực hiện.";
                        }
                        else
                        {
                            noiDungCaregiver =
                                $"Khách hàng đã hủy "
                                + $"lịch #DL{datLich.MaDatLich}. "
                                + $"Lý do: {lyDo}.";
                        }


                        _context.ThongBaos.Add(
                            new ThongBao
                            {
                                MaTaiKhoan =
                                    nguoiChamSoc
                                        .MaTaiKhoan
                                        .Value,

                                TieuDe =
                                    chinhSach.HuyGiuaChung
                                        ? "Khách hàng yêu cầu dừng chăm sóc"
                                        : "Khách hàng đã hủy lịch",

                                NoiDung =
                                    noiDungCaregiver,

                                DaDoc =
                                    false,

                                NgayGui =
                                    thoiDiemHuy
                            });
                    }
                }


                // =============================================
                // 5. THÔNG BÁO CHO KHÁCH
                // =============================================

                string noiDungHoanTien =
                    chinhSach.SoTien > 0

                        ? $"Theo chính sách hiện tại, "
                          + $"bạn được hoàn "
                          + $"{chinhSach.TyLe:0}% "
                          + $"tương đương "
                          + $"{chinhSach.SoTien:#,##0}đ."

                        : "Theo chính sách hiện tại, "
                          + "lịch này không được hoàn phí.";


                _context.ThongBaos.Add(
                    new ThongBao
                    {
                        MaTaiKhoan =
                            maTaiKhoan.Value,

                        TieuDe =
                            chinhSach.HuyGiuaChung
                                ? "Đã ghi nhận yêu cầu dừng lịch"
                                : "Hủy lịch thành công",

                        NoiDung =
                            $"Lịch #DL{datLich.MaDatLich} "
                            + "đã được hủy. "
                            + noiDungHoanTien,

                        DaDoc =
                            false,

                        NgayGui =
                            thoiDiemHuy
                    });


                /// =====================================================
                // 6. THÔNG BÁO CHO ADMIN
                // =====================================================

                var danhSachAdmin =
                    await (
                        from taiKhoan
                            in _context.TaiKhoans
                                .AsNoTracking()

                        join vaiTro
                            in _context.VaiTros
                                .AsNoTracking()

                            on taiKhoan.MaVaiTro
                            equals vaiTro.MaVaiTro

                        where taiKhoan.TrangThai
                            &&
                            vaiTro.TenVaiTro
                                == "Quản trị viên"

                        select taiKhoan.MaTaiKhoan
                    )
                    .ToListAsync();


                foreach (int maAdmin in danhSachAdmin)
                {
                    string tieuDeAdmin;


                    // -------------------------------------------------
                    // XÁC ĐỊNH TIÊU ĐỀ THÔNG BÁO
                    // -------------------------------------------------

                    if (chinhSach.HuyGiuaChung)
                    {
                        tieuDeAdmin =
                            "Khách hàng dừng lịch giữa chừng";
                    }
                    else if (chinhSach.SoTien > 0)
                    {
                        tieuDeAdmin =
                            "Có yêu cầu hoàn phí";
                    }
                    else
                    {
                        tieuDeAdmin =
                            "Khách hàng đã hủy lịch";
                    }


                    // -------------------------------------------------
                    // NỘI DUNG THÔNG BÁO
                    // -------------------------------------------------

                    string noiDungAdmin =
                        $"Lịch #DL{datLich.MaDatLich} "
                        + "đã bị khách hàng hủy. "
                        + $"Lý do: {lyDo}. "
                        + $"Tổng giá trị lịch: "
                        + $"{tongTien:#,##0}đ. "
                        + $"Khách đã thanh toán: "
                        + $"{soTienDaThanhToan:#,##0}đ. "
                        + $"Chính sách hoàn: "
                        + $"{chinhSach.TyLe:0}%. "
                        + $"Số tiền cần hoàn: "
                        + $"{chinhSach.SoTien:#,##0}đ.";


                    // -------------------------------------------------
                    // NẾU KHÁCH CÓ NHẬP GHI CHÚ
                    // -------------------------------------------------

                    if (!string.IsNullOrWhiteSpace(ghiChu))
                    {
                        noiDungAdmin +=
                            $" Ghi chú của khách: {ghiChu}.";
                    }


                    // -------------------------------------------------
                    // TẠO THÔNG BÁO
                    // -------------------------------------------------

                    _context.ThongBaos.Add(
                        new ThongBao
                        {
                            MaTaiKhoan =
                                maAdmin,

                            TieuDe =
                                tieuDeAdmin,

                            NoiDung =
                                noiDungAdmin,

                            DaDoc =
                                false,

                            NgayGui =
                                thoiDiemHuy
                        });
                }

                // =============================================
                // 7. LƯU
                // =============================================

                await _context
                    .SaveChangesAsync();


                await transaction
                    .CommitAsync();
            }
            catch
            {
                await transaction
                    .RollbackAsync();


                TempData["Error"] =
                    "Không thể hoàn tất yêu cầu hủy lịch. "
                    + "Vui lòng thử lại.";


                return RedirectToAction(
                    nameof(HuyLich),
                    new
                    {
                        id =
                            model.MaDatLich
                    });
            }


            // -------------------------------------------------
            // THÔNG BÁO THÀNH CÔNG
            // -------------------------------------------------

            if (chinhSach.HuyGiuaChung)
            {
                TempData["Success"] =
                    $"Đã ghi nhận yêu cầu dừng "
                    + $"lịch #DL{datLich.MaDatLich}. "
                    + "Theo chính sách hiện tại, "
                    + "lịch này không được hoàn phí.";
            }
            else if (chinhSach.SoTien > 0)
            {
                TempData["Success"] =
                    $"Đã hủy lịch #DL{datLich.MaDatLich}. "
                    + $"Số tiền dự kiến được hoàn: "
                    + $"{chinhSach.SoTien:#,##0}đ.";
            }
            else
            {
                TempData["Success"] =
                    $"Đã hủy lịch #DL{datLich.MaDatLich}. "
                    + "Theo chính sách hiện tại, "
                    + "lịch này không được hoàn phí.";
            }


            return RedirectToAction(
                nameof(Index));
        }



        // =====================================================
        // TÌM NGƯỜI CHĂM SÓC PHÙ HỢP
        // =====================================================

        [HttpGet]
        public async Task<IActionResult>
            TimNguoiChamSoc(
                int? maBenhNhan,
                int? maDichVu,
                DateTime? ngayChamSoc,
                TimeSpan? gioBatDau,
                string? diaChiChamSoc,
                string? ghiChu)
        {
            var khachHang =
                await LayKhachHangDangNhap();


            if (khachHang == null)
            {
                return Unauthorized();
            }


            // -------------------------------------------------
            // INPUT
            // -------------------------------------------------

            if (!maBenhNhan.HasValue
                ||
                !maDichVu.HasValue
                ||
                !ngayChamSoc.HasValue
                ||
                !gioBatDau.HasValue)
            {
                return Json(
                    new
                    {
                        thanhCong =
                            false,

                        thongBao =
                            "Vui lòng chọn người cần chăm sóc, "
                            + "dịch vụ, ngày và giờ trước."
                    });
            }


            // -------------------------------------------------
            // BỆNH NHÂN
            // -------------------------------------------------

            var benhNhan =
                await _context.BenhNhans
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x =>
                        x.MaBenhNhan
                            == maBenhNhan.Value
                        &&
                        x.MaKhachHang
                            == khachHang.MaKhachHang
                        &&
                        x.TrangThaiHoatDong);


            if (benhNhan == null)
            {
                return Json(
                    new
                    {
                        thanhCong =
                            false,

                        thongBao =
                            "Hồ sơ người cần chăm sóc "
                            + "không hợp lệ."
                    });
            }


            // -------------------------------------------------
            // DỊCH VỤ
            // -------------------------------------------------

            var dichVu =
                await _context.DichVus
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x =>
                        x.MaDichVu
                            == maDichVu.Value
                        &&
                        x.TrangThai);


            if (dichVu == null)
            {
                return Json(
                    new
                    {
                        thanhCong =
                            false,

                        thongBao =
                            "Dịch vụ không còn hoạt động."
                    });
            }


            TimeSpan gioKetThuc =
                gioBatDau.Value.Add(
                    TimeSpan.FromMinutes(
                        dichVu.ThoiLuong));


            if (gioKetThuc.TotalHours >= 24)
            {
                return Json(
                    new
                    {
                        thanhCong =
                            false,

                        thongBao =
                            "Khung giờ chăm sóc không hợp lệ."
                    });
            }


            var request =
                new CaregiverMatchRequest
                {
                    MaBenhNhan =
                        benhNhan.MaBenhNhan,

                    MaDichVu =
                        dichVu.MaDichVu,

                    NgayChamSoc =
                        ngayChamSoc.Value.Date,

                    GioBatDau =
                        gioBatDau.Value,

                    GioKetThuc =
                        gioKetThuc,

                    DiaChiChamSoc =
                        diaChiChamSoc,

                    GhiChu =
                        ghiChu,

                    /*
                     * Đây là API cho phần "Tôi muốn tự chọn".
                     * Không ép caregiver phải có LichLamViec.
                     * Danh sách vẫn loại caregiver bị trùng ca.
                     */
                    BatBuocCoLichKhaDung =
                        false
                };


            var ketQua =
                await _matchingService
                    .TimDanhSachAsync(
                        request);


            /*
             * Màn hình "Tôi muốn tự chọn" phải hiển thị
             * tất cả caregiver còn đủ điều kiện nghiệp vụ:
             * đang hoạt động, có lịch khả dụng và không trùng ca.
             *
             * DatNguongGoiY chỉ dùng cho chế độ TỰ ĐỘNG.
             * Không dùng ngưỡng thuật toán để ẩn caregiver
             * khi khách hàng chủ động tự chọn.
             */
            var danhSach =
                ketQua.DanhSach
                    .Take(20)
                    .Select(x =>
                        new
                        {
                            maNguoiChamSoc =
                                x.MaNguoiChamSoc,

                            hoTen =
                                x.HoTen,

                            chuyenMon =
                                x.ChuyenMon,

                            khuVuc =
                                x.KhuVucHoatDong,

                            kinhNghiem =
                                x.KinhNghiem,

                            danhGia =
                                x.DanhGia,

                            giaTheoGio =
                                x.GiaTheoGio,

                            diemPhuHop =
                                x.DiemPhuHop,

                            lyDoGoiY =
                                x.LyDoGoiY,

                            thuHang =
                                x.ThuHang,

                            laGoiY =
                                x.LaGoiY
                        })
                    .ToList();


            return Json(
                new
                {
                    thanhCong =
                        true,

                    dangApDungTuDong =
                        ketQua.DangApDungGoiY,

                    soLuong =
                        danhSach.Count,

                    danhSach
                });
        }



        // =====================================================
        // MỞ FORM ĐẶT LỊCH
        // =====================================================

        [HttpGet]
        public async Task<IActionResult> Create(
            int? maBenhNhan)
        {
            var khachHang =
                await LayKhachHangDangNhap();


            if (khachHang == null)
            {
                return RedirectToAction(
                    "Login",
                    "Account",
                    new
                    {
                        area = ""
                    });
            }


            var model =
                new CustomerBookingCreateViewModel
                {
                    MaBenhNhan =
                        maBenhNhan,

                    TenKhachHang =
                        khachHang.HoTen,

                    SoDienThoaiKhachHang =
                        khachHang.SoDienThoai
                        ?? "Chưa cập nhật",

                    NgayChamSoc =
                        DateTime.Today.AddDays(1),

                    GioBatDau =
                        new TimeSpan(
                            8,
                            0,
                            0),

                    HinhThucPhanCong =
                        "Tự động",

                    TyLeDatCoc =
                        PaymentPolicy.DepositPercent
                };


            await NapDuLieuFormDatLich(
                model,
                khachHang.MaKhachHang);


            if (maBenhNhan.HasValue)
            {
                var benhNhan =
                    await _context.BenhNhans
                        .AsNoTracking()
                        .FirstOrDefaultAsync(x =>
                            x.MaBenhNhan
                                == maBenhNhan.Value
                            &&
                            x.MaKhachHang
                                == khachHang.MaKhachHang
                            &&
                            x.TrangThaiHoatDong);


                if (benhNhan == null)
                {
                    TempData["Error"] =
                        "Hồ sơ người được chăm sóc "
                        + "không hợp lệ.";


                    return RedirectToAction(
                        "Index",
                        "Relatives",
                        new
                        {
                            area = "Customer"
                        });
                }


                model.DiaChiChamSoc =
                    benhNhan.DiaChi
                    ??
                    khachHang.DiaChi
                    ??
                    string.Empty;
            }
            else
            {
                model.DiaChiChamSoc =
                    khachHang.DiaChi
                    ??
                    string.Empty;
            }


            return View(model);
        }



        // =====================================================
        // TẠO LỊCH
        // =====================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            CustomerBookingCreateViewModel model)
        {
            // =================================================
            // 1. KHÁCH HÀNG
            // =================================================

            int? maTaiKhoan =
                LayMaTaiKhoan();


            if (!maTaiKhoan.HasValue)
            {
                return RedirectToAction(
                    "Login",
                    "Account",
                    new
                    {
                        area = ""
                    });
            }


            var khachHang =
                await _context.KhachHangs
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x =>
                        x.MaTaiKhoan
                            == maTaiKhoan.Value);


            if (khachHang == null)
            {
                return RedirectToAction(
                    "AccessDenied",
                    "Account",
                    new
                    {
                        area = ""
                    });
            }


            // =================================================
            // 2. BỆNH NHÂN
            // =================================================

            BenhNhan? benhNhan =
                null;


            if (!model.MaBenhNhan.HasValue)
            {
                ModelState.AddModelError(
                    nameof(model.MaBenhNhan),
                    "Vui lòng chọn người cần chăm sóc.");
            }
            else
            {
                benhNhan =
                    await _context.BenhNhans
                        .AsNoTracking()
                        .FirstOrDefaultAsync(x =>
                            x.MaBenhNhan
                                == model.MaBenhNhan.Value
                            &&
                            x.MaKhachHang
                                == khachHang.MaKhachHang
                            &&
                            x.TrangThaiHoatDong);


                if (benhNhan == null)
                {
                    ModelState.AddModelError(
                        nameof(model.MaBenhNhan),
                        "Hồ sơ người cần chăm sóc "
                        + "không hợp lệ.");
                }
            }


            // =================================================
            // 3. DỊCH VỤ
            // =================================================

            DichVu? dichVu =
                null;


            if (!model.MaDichVu.HasValue)
            {
                ModelState.AddModelError(
                    nameof(model.MaDichVu),
                    "Vui lòng chọn dịch vụ chăm sóc.");
            }
            else
            {
                dichVu =
                    await _context.DichVus
                        .AsNoTracking()
                        .FirstOrDefaultAsync(x =>
                            x.MaDichVu
                                == model.MaDichVu.Value
                            &&
                            x.TrangThai);


                if (dichVu == null)
                {
                    ModelState.AddModelError(
                        nameof(model.MaDichVu),
                        "Dịch vụ không tồn tại "
                        + "hoặc đã ngừng hoạt động.");
                }
            }


            // =================================================
            // 4. NGÀY GIỜ
            // =================================================

            TimeSpan? gioKetThuc =
                null;


            if (!model.NgayChamSoc.HasValue)
            {
                ModelState.AddModelError(
                    nameof(model.NgayChamSoc),
                    "Vui lòng chọn ngày chăm sóc.");
            }


            if (!model.GioBatDau.HasValue)
            {
                ModelState.AddModelError(
                    nameof(model.GioBatDau),
                    "Vui lòng chọn giờ bắt đầu.");
            }


            if (model.NgayChamSoc.HasValue
                &&
                model.GioBatDau.HasValue)
            {
                DateTime thoiDiemBatDau =
                    model.NgayChamSoc
                        .Value.Date
                        .Add(
                            model.GioBatDau.Value);


                if (thoiDiemBatDau
                    <= DateTime.Now)
                {
                    ModelState.AddModelError(
                        nameof(model.NgayChamSoc),
                        "Thời gian chăm sóc phải lớn hơn "
                        + "thời điểm hiện tại.");
                }


                if (dichVu != null)
                {
                    TimeSpan ketThucTinhToan =
                        model.GioBatDau
                            .Value
                            .Add(
                                TimeSpan.FromMinutes(
                                    dichVu.ThoiLuong));


                    if (ketThucTinhToan.TotalHours
                        >= 24)
                    {
                        ModelState.AddModelError(
                            nameof(model.GioBatDau),
                            "Giờ bắt đầu quá muộn. "
                            + "Buổi chăm sóc sẽ kéo dài "
                            + "sang ngày hôm sau.");
                    }
                    else
                    {
                        gioKetThuc =
                            ketThucTinhToan;
                    }
                }
            }


            // =================================================
            // 5. ĐỊA CHỈ
            // =================================================

            if (string.IsNullOrWhiteSpace(
                    model.DiaChiChamSoc))
            {
                ModelState.AddModelError(
                    nameof(model.DiaChiChamSoc),
                    "Vui lòng nhập địa chỉ chăm sóc.");
            }
            else if (
                model.DiaChiChamSoc
                    .Trim()
                    .Length
                > 500)
            {
                ModelState.AddModelError(
                    nameof(model.DiaChiChamSoc),
                    "Địa chỉ chăm sóc không được "
                    + "vượt quá 500 ký tự.");
            }


            // =================================================
            // 6. HÌNH THỨC PHÂN CÔNG
            // =================================================

            string[] hinhThucPhanCongHopLe =
            {
                "Tự động",
                "Khách hàng chọn"
            };


            if (string.IsNullOrWhiteSpace(
                    model.HinhThucPhanCong)
                ||
                !hinhThucPhanCongHopLe.Contains(
                    model.HinhThucPhanCong))
            {
                ModelState.AddModelError(
                    nameof(model.HinhThucPhanCong),
                    "Vui lòng chọn hình thức "
                    + "phân công người chăm sóc.");
            }


            if (model.HinhThucPhanCong
                    == "Khách hàng chọn"
                &&
                !model.MaNguoiChamSoc.HasValue)
            {
                ModelState.AddModelError(
                    nameof(model.MaNguoiChamSoc),
                    "Vui lòng chọn một người chăm sóc.");
            }


            // =================================================
            // 7. THANH TOÁN
            // =================================================

            string[] loaiThanhToanHopLe =
            {
                "Đặt cọc",
                "Thanh toán toàn bộ"
            };


            if (string.IsNullOrWhiteSpace(
                    model.LoaiThanhToan)
                ||
                !loaiThanhToanHopLe.Contains(
                    model.LoaiThanhToan))
            {
                ModelState.AddModelError(
                    nameof(model.LoaiThanhToan),
                    "Vui lòng chọn đặt cọc "
                    + "hoặc thanh toán toàn bộ.");
            }


            string[] phuongThucHopLe =
            {
                "Chuyển khoản",
                "VNPay"
            };


            if (string.IsNullOrWhiteSpace(
                    model.PhuongThucThanhToan)
                ||
                !phuongThucHopLe.Contains(
                    model.PhuongThucThanhToan))
            {
                ModelState.AddModelError(
                    nameof(model.PhuongThucThanhToan),
                    "Vui lòng chọn phương thức "
                    + "thanh toán.");
            }


            // =================================================
            // 8. DỮ LIỆU HIỂN THỊ
            // =================================================

            model.TenKhachHang =
                khachHang.HoTen;


            model.SoDienThoaiKhachHang =
                khachHang.SoDienThoai
                ?? "Chưa cập nhật";


            model.TyLeDatCoc =
                PaymentPolicy.DepositPercent;


            if (dichVu != null)
            {
                model.ThoiLuong =
                    dichVu.ThoiLuong;

                model.DonGia =
                    dichVu.Gia;

                model.GioKetThuc =
                    gioKetThuc;

                model.TongTien =
                    dichVu.Gia;
            }


            // =================================================
            // 9. FORM SAI
            // =================================================

            if (!ModelState.IsValid)
            {
                await NapDuLieuFormDatLich(
                    model,
                    khachHang.MaKhachHang);


                return View(model);
            }


            var matchRequest =
                new CaregiverMatchRequest
                {
                    MaBenhNhan =
                        benhNhan!.MaBenhNhan,

                    MaDichVu =
                        dichVu!.MaDichVu,

                    NgayChamSoc =
                        model.NgayChamSoc!
                            .Value.Date,

                    GioBatDau =
                        model.GioBatDau!
                            .Value,

                    GioKetThuc =
                        gioKetThuc!
                            .Value,

                    DiaChiChamSoc =
                        model.DiaChiChamSoc
                            .Trim(),

                    GhiChu =
                        string.IsNullOrWhiteSpace(
                            model.GhiChu)

                            ? null

                            : model.GhiChu.Trim(),

                    /*
                     * Tự động phải tôn trọng lịch khả dụng.
                     * Khách tự chọn chỉ cần caregiver đang
                     * hoạt động và không trùng lịch khác.
                     */
                    BatBuocCoLichKhaDung =
                        model.HinhThucPhanCong
                            == "Tự động"
                };


            // =================================================
            // TRANSACTION SERIALIZABLE
            // =================================================

            await using var transaction =
                await _context.Database
                    .BeginTransactionAsync(
                        IsolationLevel.Serializable);


            try
            {
                // =============================================
                // 10. CHỌN CAREGIVER
                // =============================================

                CaregiverMatchItem?
                    caregiverDuocChon;


                if (model.HinhThucPhanCong
                    == "Tự động")
                {
                    caregiverDuocChon =
                        await _matchingService
                            .TimNguoiTotNhatAsync(
                                matchRequest);


                    if (caregiverDuocChon == null)
                    {
                        await transaction
                            .RollbackAsync();


                        ModelState.AddModelError(
                            nameof(
                                model.HinhThucPhanCong),

                            "Hiện chưa có người chăm sóc "
                            + "phù hợp và còn trống "
                            + "trong khung giờ đã chọn.");


                        await NapDuLieuFormDatLich(
                            model,
                            khachHang.MaKhachHang);


                        return View(model);
                    }
                }
                else
                {
                    caregiverDuocChon =
                        await _matchingService
                            .KiemTraLuaChonAsync(
                                model.MaNguoiChamSoc!
                                    .Value,
                                matchRequest);


                    if (caregiverDuocChon == null)
                    {
                        await transaction
                            .RollbackAsync();


                        ModelState.AddModelError(
                            nameof(
                                model.MaNguoiChamSoc),

                            "Người chăm sóc bạn chọn "
                            + "không còn khả dụng. "
                            + "Có thể họ vừa nhận "
                            + "một lịch khác. "
                            + "Vui lòng chọn lại.");


                        model.MaNguoiChamSoc =
                            null;


                        await NapDuLieuFormDatLich(
                            model,
                            khachHang.MaKhachHang);


                        return View(model);
                    }
                }


                // =============================================
                // 11. TÀI KHOẢN CAREGIVER
                // =============================================

                var nguoiChamSoc =
                    await _context.NguoiChamSocs
                        .AsNoTracking()
                        .FirstOrDefaultAsync(x =>
                            x.MaNguoiChamSoc
                                == caregiverDuocChon
                                    .MaNguoiChamSoc);


                if (nguoiChamSoc == null
                    ||
                    !nguoiChamSoc.MaTaiKhoan
                        .HasValue)
                {
                    await transaction
                        .RollbackAsync();


                    ModelState.AddModelError(
                        string.Empty,
                        "Không thể xác định tài khoản "
                        + "người chăm sóc.");


                    await NapDuLieuFormDatLich(
                        model,
                        khachHang.MaKhachHang);


                    return View(model);
                }


                // =============================================
                // 12. TÍNH TIỀN
                // =============================================

                decimal tongTien =
                    dichVu.Gia;


                decimal phiNenTang =
                    decimal.Round(
                        tongTien
                        *
                        PaymentPolicy
                            .PlatformFeePercent
                        /
                        100m,
                        0,
                        MidpointRounding
                            .AwayFromZero);


                decimal thuNhapNguoiChamSoc =
                    tongTien
                    -
                    phiNenTang;


                // =============================================
                // 13. TẠO LỊCH
                // =============================================

                var datLich =
                    new DatLich
                    {
                        MaKhachHang =
                            khachHang.MaKhachHang,

                        MaBenhNhan =
                            benhNhan.MaBenhNhan,

                        MaNguoiChamSoc =
                            caregiverDuocChon
                                .MaNguoiChamSoc,

                        MaDichVu =
                            dichVu.MaDichVu,

                        NgayChamSoc =
                            model.NgayChamSoc
                                .Value.Date,

                        GioBatDau =
                            model.GioBatDau
                                .Value,

                        GioKetThuc =
                            gioKetThuc
                                .Value,

                        DiaChiChamSoc =
                            model.DiaChiChamSoc
                                .Trim(),

                        TongTien =
                            tongTien,

                        TyLePhiNenTang =
                            PaymentPolicy
                                .PlatformFeePercent,

                        PhiNenTang =
                            phiNenTang,

                        ThuNhapNguoiChamSoc =
                            thuNhapNguoiChamSoc,

                        HinhThucPhanCong =
                            model.HinhThucPhanCong,

                        DiemPhuHopPhanCong =
                            (decimal)
                            caregiverDuocChon
                                .DiemPhuHop,

                        NgayPhanCong =
                            DateTime.Now,

                        TrangThai =
                            BookingStatus
                                .ChoXacNhan,

                        GhiChu =
                            string.IsNullOrWhiteSpace(
                                model.GhiChu)

                                ? null

                                : model.GhiChu.Trim(),

                        NgayDat =
                            DateTime.Now
                    };


                _context.DatLichs.Add(
                    datLich);


                await _context
                    .SaveChangesAsync();


                // =============================================
                // 14. THANH TOÁN
                // =============================================

                DateTime bayGio =
                    DateTime.Now;


                decimal soTienThanhToan;

                string trangThaiThanhToan;

                DateTime? ngayDatCoc =
                    null;

                DateTime? ngayThanhToan =
                    null;


                if (model.LoaiThanhToan
                    == "Đặt cọc")
                {
                    soTienThanhToan =
                        decimal.Round(
                            tongTien
                            *
                            PaymentPolicy
                                .DepositPercent
                            /
                            100m,
                            0,
                            MidpointRounding
                                .AwayFromZero);


                    trangThaiThanhToan =
                        "Đã đặt cọc";


                    ngayDatCoc =
                        bayGio;
                }
                else
                {
                    soTienThanhToan =
                        tongTien;


                    trangThaiThanhToan =
                        "Đã thanh toán";


                    ngayThanhToan =
                        bayGio;
                }


                var thanhToan =
                    new ThanhToan
                    {
                        MaDatLich =
                            datLich.MaDatLich,

                        SoTien =
                            soTienThanhToan,

                        TyLeDatCoc =
                            PaymentPolicy
                                .DepositPercent,

                        LoaiThanhToan =
                            model.LoaiThanhToan,

                        PhuongThucThanhToan =
                            model.PhuongThucThanhToan,

                        TrangThai =
                            trangThaiThanhToan,

                        NgayDatCoc =
                            ngayDatCoc,

                        NgayThanhToan =
                            ngayThanhToan,

                        NgayTao =
                            bayGio
                    };


                _context.ThanhToans.Add(
                    thanhToan);


                // =============================================
                // 15. THÔNG BÁO KHÁCH
                // =============================================

                string noiDungThanhToan =
                    model.LoaiThanhToan
                        == "Đặt cọc"

                        ? $"Đã đặt cọc "
                          + $"{PaymentPolicy.DepositPercent:0}% "
                          + $"({soTienThanhToan:#,##0}đ)."

                        : $"Đã thanh toán toàn bộ "
                          + $"({soTienThanhToan:#,##0}đ).";


                _context.ThongBaos.Add(
                    new ThongBao
                    {
                        MaTaiKhoan =
                            maTaiKhoan.Value,

                        TieuDe =
                            "Đặt lịch chăm sóc thành công",

                        NoiDung =
                            $"Lịch #{datLich.MaDatLich} "
                            + $"cho {benhNhan.HoTen} "
                            + $"vào ngày "
                            + $"{datLich.NgayChamSoc:dd/MM/yyyy}, "
                            + $"lúc "
                            + $"{datLich.GioBatDau:hh\\:mm} "
                            + $"đã được phân công cho "
                            + $"{caregiverDuocChon.HoTen}. "
                            + $"{noiDungThanhToan} "
                            + "Đang chờ người chăm sóc "
                            + "xác nhận nhận lịch.",

                        DaDoc =
                            false,

                        NgayGui =
                            bayGio
                    });


                // =============================================
                // 16. THÔNG BÁO CAREGIVER
                // =============================================

                _context.ThongBaos.Add(
                    new ThongBao
                    {
                        MaTaiKhoan =
                            nguoiChamSoc
                                .MaTaiKhoan
                                .Value,

                        TieuDe =
                            "Bạn có lịch chăm sóc mới",

                        NoiDung =
                            $"Bạn vừa được phân công "
                            + $"lịch #{datLich.MaDatLich} "
                            + $"vào ngày "
                            + $"{datLich.NgayChamSoc:dd/MM/yyyy}, "
                            + $"từ "
                            + $"{datLich.GioBatDau:hh\\:mm} "
                            + $"đến "
                            + $"{datLich.GioKetThuc:hh\\:mm}. "
                            + "Vui lòng kiểm tra "
                            + "và xác nhận nhận lịch.",

                        DaDoc =
                            false,

                        NgayGui =
                            bayGio
                    });


                // =============================================
                // 17. SAVE
                // =============================================

                await _context
                    .SaveChangesAsync();


                await transaction
                    .CommitAsync();


                // =============================================
                // 18. SUCCESS
                // =============================================

                string thongTinPhanCong =
                    model.HinhThucPhanCong
                        == "Tự động"

                        ? "Hệ thống đã tự động chọn "
                          + $"{caregiverDuocChon.HoTen} "
                          + $"với mức phù hợp "
                          + $"{caregiverDuocChon.DiemPhuHop:0.0}%."

                        : $"Bạn đã chọn "
                          + $"{caregiverDuocChon.HoTen}.";


                string thongTinThanhToan =
                    model.LoaiThanhToan
                        == "Đặt cọc"

                        ? $"Đã đặt cọc "
                          + $"{soTienThanhToan:#,##0}đ."

                        : $"Đã thanh toán "
                          + $"{soTienThanhToan:#,##0}đ.";


                TempData["Success"] =
                    "Đặt lịch thành công. "
                    + thongTinPhanCong
                    + " "
                    + thongTinThanhToan
                    + $" Mã lịch #{datLich.MaDatLich}. "
                    + "Đang chờ người chăm sóc xác nhận.";


                return RedirectToAction(
                    nameof(Details),
                    new
                    {
                        id =
                            datLich.MaDatLich
                    });
            }
            catch (DbUpdateException)
            {
                await transaction
                    .RollbackAsync();


                ModelState.AddModelError(
                    string.Empty,
                    "Không thể lưu lịch chăm sóc "
                    + "vào cơ sở dữ liệu. "
                    + "Vui lòng thử lại.");
            }
            catch (Exception)
            {
                await transaction
                    .RollbackAsync();


                ModelState.AddModelError(
                    string.Empty,
                    "Đã xảy ra lỗi khi tạo lịch chăm sóc. "
                    + "Vui lòng thử lại.");
            }


            await NapDuLieuFormDatLich(
                model,
                khachHang.MaKhachHang);


            return View(model);
        }



        // =====================================================
        // DANH SÁCH LỊCH CỦA KHÁCH HÀNG
        // =====================================================

        [HttpGet]
        public async Task<IActionResult> Index(
            string? trangThai)
        {
            int? maKhachHang =
                await LayMaKhachHangDangNhap();


            if (!maKhachHang.HasValue)
            {
                TempData["Error"] =
                    "Không tìm thấy hồ sơ khách hàng.";


                return RedirectToAction(
                    "Index",
                    "Dashboard",
                    new
                    {
                        area = "Customer"
                    });
            }


            trangThai =
                trangThai?.Trim()
                ?? string.Empty;


            var query =
                from lich
                    in _context.DatLichs
                        .AsNoTracking()

                join benhNhan
                    in _context.BenhNhans
                        .AsNoTracking()

                    on lich.MaBenhNhan
                    equals benhNhan.MaBenhNhan

                join dichVu
                    in _context.DichVus
                        .AsNoTracking()

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
                    in nhomNguoiChamSoc
                        .DefaultIfEmpty()

                join huyLichTam
                    in _context.HuyLichs
                        .AsNoTracking()

                    on lich.MaDatLich
                    equals huyLichTam.MaDatLich

                    into nhomHuyLich

                from huyLich
                    in nhomHuyLich
                        .DefaultIfEmpty()

                where lich.MaKhachHang
                    == maKhachHang.Value

                select new CustomerBookingItemViewModel
                {
                    MaDatLich =
                        lich.MaDatLich,

                    TenBenhNhan =
                        string.IsNullOrWhiteSpace(
                            benhNhan.HoTen)

                            ? "Người được chăm sóc"

                            : benhNhan.HoTen,

                    TenDichVu =
                        string.IsNullOrWhiteSpace(
                            dichVu.TenDichVu)

                            ? "Dịch vụ chăm sóc"

                            : dichVu.TenDichVu,

                    TenNguoiChamSoc =
                        nguoiChamSoc == null

                            ? "Chưa phân công"

                            : nguoiChamSoc.HoTen,

                    KhuVuc =
                        string.IsNullOrWhiteSpace(
                            benhNhan.DiaChi)

                            ? "Chưa cập nhật"

                            : benhNhan.DiaChi,

                    NgayChamSoc =
                        lich.NgayChamSoc
                        ?? DateTime.Today,

                    GioBatDau =
                        lich.GioBatDau
                        ?? TimeSpan.Zero,

                    GioKetThuc =
                        lich.GioKetThuc
                        ?? TimeSpan.Zero,

                    TongTien =
                        lich.TongTien
                        ?? 0m,

                    TrangThai =
                        lich.TrangThai
                        ?? "Chưa xác định",


                    /*
                     * Ưu tiên bảng HuyLich.
                     */
                    LyDoHuy =
                        huyLich != null
                            ? huyLich.LyDo
                            : lich.LyDoHuy,


                    NguoiHuy =
                        huyLich != null
                            ? "Khách hàng"
                            : lich.NguoiHuy,


                    NgayHuy =
                        huyLich != null
                            ? (DateTime?)
                                huyLich.ThoiDiemHuy
                            : lich.NgayHuy
                };


            if (!string.IsNullOrWhiteSpace(
                    trangThai))
            {
                query =
                    query.Where(x =>
                        x.TrangThai
                            == trangThai);
            }


            var danhSachLich =
                await query
                    .OrderBy(x =>
                        x.TrangThai
                            == BookingStatus.ChoXacNhan

                            ? 0

                            : 1)
                    .ThenByDescending(x =>
                        x.NgayChamSoc)
                    .ThenBy(x =>
                        x.GioBatDau)
                    .ToListAsync();


            var tatCaLich =
                _context.DatLichs
                    .AsNoTracking()
                    .Where(x =>
                        x.MaKhachHang
                            == maKhachHang.Value);


            var model =
                new CustomerBookingViewModel
                {
                    TongLich =
                        await tatCaLich
                            .CountAsync(),

                    ChoXacNhan =
                        await tatCaLich
                            .CountAsync(x =>
                                x.TrangThai
                                    == BookingStatus
                                        .ChoXacNhan),

                    DaXacNhan =
                        await tatCaLich
                            .CountAsync(x =>
                                x.TrangThai
                                    == BookingStatus
                                        .DaXacNhan),

                    DangThucHien =
                        await tatCaLich
                            .CountAsync(x =>
                                x.TrangThai
                                    == BookingStatus
                                        .DangThucHien),

                    DaHoanThanh =
                        await tatCaLich
                            .CountAsync(x =>
                                x.TrangThai
                                    == BookingStatus
                                        .DaHoanThanh),

                    DaHuy =
                        await tatCaLich
                            .CountAsync(x =>
                                x.TrangThai
                                    == BookingStatus
                                        .DaHuy),

                    TrangThai =
                        trangThai,

                    DanhSachLich =
                        danhSachLich
                };


            return View(model);
        }



        // =====================================================
        // CHI TIẾT LỊCH
        // =====================================================

        [HttpGet]
        public async Task<IActionResult> Details(
            int id)
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
                        in _context.DatLichs
                            .AsNoTracking()

                    join khachHang
                        in _context.KhachHangs
                            .AsNoTracking()

                        on lich.MaKhachHang
                        equals khachHang.MaKhachHang

                    join benhNhan
                        in _context.BenhNhans
                            .AsNoTracking()

                        on lich.MaBenhNhan
                        equals benhNhan.MaBenhNhan

                    join dichVu
                        in _context.DichVus
                            .AsNoTracking()

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
                        in nhomNguoiChamSoc
                            .DefaultIfEmpty()

                    join huyLichTam
                        in _context.HuyLichs
                            .AsNoTracking()

                        on lich.MaDatLich
                        equals huyLichTam.MaDatLich

                        into nhomHuyLich

                    from huyLich
                        in nhomHuyLich
                            .DefaultIfEmpty()

                    where lich.MaDatLich
                            == id
                        &&
                        lich.MaKhachHang
                            == maKhachHang.Value

                    select new
                        CustomerBookingDetailsViewModel
                    {
                        MaDatLich =
                            lich.MaDatLich,

                        TenKhachHang =
                            khachHang.HoTen,

                        TenBenhNhan =
                            benhNhan.HoTen,

                        TenDichVu =
                            dichVu.TenDichVu,

                        MoTaDichVu =
                            dichVu.MoTa
                            ?? string.Empty,

                        TenNguoiChamSoc =
                            nguoiChamSoc == null

                                ? "Chưa phân công"

                                : nguoiChamSoc.HoTen,

                        SoDienThoaiNguoiChamSoc =
                            nguoiChamSoc == null

                                ? string.Empty

                                : nguoiChamSoc
                                    .SoDienThoai
                                    ?? string.Empty,

                        ChuyenMonNguoiChamSoc =
                            nguoiChamSoc == null

                                ? string.Empty

                                : nguoiChamSoc
                                    .ChuyenMon
                                    ?? string.Empty,

                        KhuVuc =
                            string.IsNullOrWhiteSpace(
                                benhNhan.DiaChi)

                                ? "Chưa cập nhật"

                                : benhNhan.DiaChi,

                        NgayChamSoc =
                            lich.NgayChamSoc
                            ?? DateTime.Today,

                        GioBatDau =
                            lich.GioBatDau
                            ?? TimeSpan.Zero,

                        GioKetThuc =
                            lich.GioKetThuc
                            ?? TimeSpan.Zero,

                        TongTien =
                            lich.TongTien
                            ?? 0m,

                        TrangThai =
                            lich.TrangThai
                            ?? "Chưa xác định",

                        GhiChu =
                            lich.GhiChu,



                        LyDoHuy =
                            huyLich != null
                                ? huyLich.LyDo
                                : lich.LyDoHuy,


                        NguoiHuy =
                            huyLich != null
                                ? "Khách hàng"
                                : lich.NguoiHuy,


                        NgayHuy =
                            huyLich != null
                                ? (DateTime?)
                                    huyLich.ThoiDiemHuy
                                : lich.NgayHuy
                    }
                )
                .FirstOrDefaultAsync();


            if (model == null)
            {
                return NotFound(
                    "Không tìm thấy lịch chăm sóc "
                    + "hoặc lịch không thuộc "
                    + "tài khoản của bạn.");
            }


            return View(model);
        }
    }
}