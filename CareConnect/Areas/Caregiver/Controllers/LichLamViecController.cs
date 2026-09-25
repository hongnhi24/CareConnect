using CareConnect.Constants;
using CareConnect.Data;
using CareConnect.Hubs;
using CareConnect.Models;
using CareConnect.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace CareConnect.Areas.Caregiver.Controllers
{
    [Area("Caregiver")]
    [Authorize(Roles = "Người chăm sóc")]
    public class LichLamViecController : Controller
    {
        private readonly CareConnectDbContext _context;
        private readonly IHubContext<NotificationHub> _hubContext;
        private readonly ILogger<LichLamViecController> _logger;

        private static readonly HashSet<string> LyDoTuChoiHopLe =
            new(StringComparer.OrdinalIgnoreCase)
            {
                "Trùng với lịch chăm sóc khác",
                "Không thể làm việc trong khung giờ này",
                "Khu vực chăm sóc quá xa",
                "Dịch vụ không phù hợp chuyên môn",
                "Có việc cá nhân đột xuất",
                "Không còn khả dụng trong ngày này",
                "Lý do khác"
            };

        public LichLamViecController(
            CareConnectDbContext context,
            IHubContext<NotificationHub> hubContext,
            ILogger<LichLamViecController> logger)
        {
            _context = context;
            _hubContext = hubContext;
            _logger = logger;
        }

        // =====================================================
        // HIỂN THỊ DANH SÁCH LỊCH
        // =====================================================
        [HttpGet]
        public async Task<IActionResult> Index(
            string? tuKhoa,
            string? trangThai,
            DateTime? tuNgay,
            DateTime? denNgay,
            string? locNhanh)
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

            var query =
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

                where datLich.MaNguoiChamSoc
                    == maNguoiChamSoc

                select new
                {
                    DatLich = datLich,
                    KhachHang = khachHang,
                    BenhNhan = benhNhan,
                    DichVu = dichVu
                };

            string tuKhoaDaChuanHoa =
                tuKhoa?.Trim() ?? string.Empty;

            string trangThaiDaChuanHoa =
                trangThai?.Trim() ?? string.Empty;

            string locNhanhDaChuanHoa =
                locNhanh?.Trim().ToLowerInvariant()
                ?? string.Empty;

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
                            x.DatLich.DiaChiChamSoc != null
                            &&
                            x.DatLich.DiaChiChamSoc.Contains(
                                tuKhoaDaChuanHoa)
                        ));
            }

            if (!string.IsNullOrWhiteSpace(
                    trangThaiDaChuanHoa))
            {
                query =
                    query.Where(x =>
                        x.DatLich.TrangThai
                            == trangThaiDaChuanHoa);
            }

            DateTime homNay =
                DateTime.Today;

            if (locNhanhDaChuanHoa == "cho-xac-nhan")
            {
                query =
                    query.Where(x =>
                        x.DatLich.TrangThai
                            == BookingStatus.ChoXacNhan);
            }
            else if (locNhanhDaChuanHoa == "sap-toi")
            {
                query =
                    query.Where(x =>
                        x.DatLich.NgayChamSoc.HasValue
                        &&
                        x.DatLich.NgayChamSoc.Value.Date
                            >= homNay
                        &&
                        (
                            x.DatLich.TrangThai
                                == BookingStatus.DaXacNhan
                            ||
                            x.DatLich.TrangThai
                                == BookingStatus.DangThucHien
                        ));
            }
            else if (locNhanhDaChuanHoa == "da-hoan-thanh")
            {
                query =
                    query.Where(x =>
                        x.DatLich.TrangThai
                            == BookingStatus.DaHoanThanh);
            }

            if (tuNgay.HasValue)
            {
                DateTime ngayBatDau =
                    tuNgay.Value.Date;

                query =
                    query.Where(x =>
                        x.DatLich.NgayChamSoc.HasValue
                        &&
                        x.DatLich.NgayChamSoc.Value.Date
                            >= ngayBatDau);
            }

            if (denNgay.HasValue)
            {
                DateTime ngayKetThuc =
                    denNgay.Value.Date;

                query =
                    query.Where(x =>
                        x.DatLich.NgayChamSoc.HasValue
                        &&
                        x.DatLich.NgayChamSoc.Value.Date
                            <= ngayKetThuc);
            }

            var danhSachChuaSapXep =
                await query
                    .Select(x =>
                        new CaregiverScheduleItemViewModel
                        {
                            MaDatLich =
                                x.DatLich.MaDatLich,

                            TenKhachHang =
                                x.KhachHang.HoTen,

                            SoDienThoaiKhachHang =
                                x.KhachHang.SoDienThoai
                                ?? "Chưa cập nhật",

                            TenBenhNhan =
                                x.BenhNhan.HoTen,

                            TinhTrangSucKhoe =
                                x.BenhNhan.TinhTrangSucKhoe
                                ?? "Chưa cập nhật",

                            TenDichVu =
                                x.DichVu.TenDichVu,

                            NgayChamSoc =
                                x.DatLich.NgayChamSoc
                                ?? DateTime.MinValue,

                            GioBatDau =
                                x.DatLich.GioBatDau
                                ?? TimeSpan.Zero,

                            GioKetThuc =
                                x.DatLich.GioKetThuc
                                ?? TimeSpan.Zero,

                            DiaChiChamSoc =
                                x.DatLich.DiaChiChamSoc
                                ?? "Chưa cập nhật",

                            TongTien =
                                x.DatLich.TongTien
                                ?? 0,

                            TrangThai =
                                x.DatLich.TrangThai
                                ?? "Chưa xác định",

                            GhiChu =
                                x.DatLich.GhiChu
                                ?? string.Empty
                        })
                    .ToListAsync();

            var danhSach =
                danhSachChuaSapXep
                    .OrderBy(x =>
                        x.NgayChamSoc.Date < homNay
                            ? 1
                            : 0)
                    .ThenBy(x =>
                        x.NgayChamSoc.Date < homNay
                            ? DateTime.MaxValue
                            : x.NgayChamSoc.Date)
                    .ThenBy(x =>
                        x.NgayChamSoc.Date < homNay
                            ? TimeSpan.MaxValue
                            : x.GioBatDau)
                    .ThenByDescending(x =>
                        x.NgayChamSoc.Date < homNay
                            ? x.NgayChamSoc.Date
                            : DateTime.MinValue)
                    .ThenByDescending(x =>
                        x.NgayChamSoc.Date < homNay
                            ? x.GioBatDau
                            : TimeSpan.Zero)
                    .ToList();

            var tatCaLich =
                _context.DatLichs
                    .AsNoTracking()
                    .Where(x =>
                        x.MaNguoiChamSoc
                            == maNguoiChamSoc);

            var model =
                new CaregiverScheduleViewModel
                {
                    TongLich =
                        await tatCaLich.CountAsync(),

                    ChoXacNhan =
                        await tatCaLich.CountAsync(x =>
                            x.TrangThai
                                == BookingStatus.ChoXacNhan),

                    SapToi =
                        await tatCaLich.CountAsync(x =>
                            x.NgayChamSoc.HasValue
                            &&
                            x.NgayChamSoc.Value.Date
                                >= homNay
                            &&
                            (
                                x.TrangThai
                                    == BookingStatus.DaXacNhan
                                ||
                                x.TrangThai
                                    == BookingStatus.DangThucHien
                            )),

                    DaHoanThanh =
                        await tatCaLich.CountAsync(x =>
                            x.TrangThai
                                == BookingStatus.DaHoanThanh),

                    TuKhoa =
                        tuKhoaDaChuanHoa,

                    TrangThai =
                        trangThaiDaChuanHoa,

                    TuNgay =
                        tuNgay,

                    DenNgay =
                        denNgay,

                    LocNhanh =
                        locNhanhDaChuanHoa,

                    DanhSachLich =
                        danhSach
                };

            return View(model);
        }

        // =====================================================
        // TỪ CHỐI LỊCH
        // =====================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> TuChoiLich(
            CaregiverRejectBookingViewModel model)
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

            if (!LyDoTuChoiHopLe.Contains(
                    lyDo))
            {
                ModelState.AddModelError(
                    nameof(model.LyDo),
                    "Lý do từ chối không hợp lệ.");
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
                    "Vui lòng nhập ghi chú khi chọn lý do khác.");
            }

            if (!ModelState.IsValid)
            {
                TempData["Error"] =
                    ModelState.Values
                        .SelectMany(x => x.Errors)
                        .Select(x => x.ErrorMessage)
                        .FirstOrDefault()
                    ??
                    "Thông tin từ chối lịch không hợp lệ.";

                return RedirectToAction(
                    nameof(Index));
            }

            var nguoiChamSoc =
                await _context.NguoiChamSocs
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x =>
                        x.MaTaiKhoan
                            == maTaiKhoan.Value);

            if (nguoiChamSoc == null)
            {
                TempData["Error"] =
                    "Không tìm thấy hồ sơ người chăm sóc.";

                return RedirectToAction(
                    nameof(Index));
            }

            var datLich =
                await _context.DatLichs
                    .FirstOrDefaultAsync(x =>
                        x.MaDatLich
                            == model.MaDatLich
                        &&
                        x.MaNguoiChamSoc
                            == nguoiChamSoc.MaNguoiChamSoc);

            if (datLich == null)
            {
                TempData["Error"] =
                    "Lịch này không còn được phân công cho bạn "
                    + "hoặc đã được thay đổi.";

                return RedirectToAction(
                    nameof(Index));
            }

            if (datLich.TrangThai
                != BookingStatus.ChoXacNhan)
            {
                TempData["Error"] =
                    "Chỉ có thể từ chối lịch đang chờ xác nhận.";

                return RedirectToAction(
                    nameof(Index));
            }

            bool daTuChoi =
                await _context.TuChoiLichs
                    .AsNoTracking()
                    .AnyAsync(x =>
                        x.MaDatLich
                            == datLich.MaDatLich
                        &&
                        x.MaNguoiChamSoc
                            == nguoiChamSoc.MaNguoiChamSoc);

            if (daTuChoi)
            {
                TempData["Error"] =
                    "Bạn đã từ chối lịch này trước đó.";

                return RedirectToAction(
                    nameof(Index));
            }

            _context.TuChoiLichs.Add(
                new TuChoiLich
                {
                    MaDatLich =
                        datLich.MaDatLich,

                    MaNguoiChamSoc =
                        nguoiChamSoc.MaNguoiChamSoc,

                    LyDo =
                        lyDo,

                    GhiChu =
                        ghiChu,

                    NgayTuChoi =
                        DateTime.Now
                });

            /*
             * Người chăm sóc từ chối không đồng nghĩa
             * khách hàng hủy lịch.
             */
            datLich.MaNguoiChamSoc =
                null;

            datLich.DiemPhuHopPhanCong =
                null;

            datLich.NgayPhanCong =
                null;

            datLich.TrangThai =
                BookingStatus.ChoXacNhan;

            var khachHang =
                await _context.KhachHangs
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x =>
                        x.MaKhachHang
                            == datLich.MaKhachHang);

            ThongBao? thongBao =
                null;

            if (khachHang != null)
            {
                thongBao =
                    new ThongBao
                    {
                        MaTaiKhoan =
                            khachHang.MaTaiKhoan,

                        TieuDe =
                            "Lịch chăm sóc cần phân công lại",

                        NoiDung =
                            $"Người chăm sóc "
                            + $"{nguoiChamSoc.HoTen ?? "được phân công"} "
                            + $"không thể nhận lịch "
                            + $"#DL{datLich.MaDatLich}. "
                            + $"Lý do: {lyDo}. "
                            + "CareConnect sẽ hỗ trợ phân công "
                            + "người chăm sóc khác.",

                        DaDoc =
                            false,

                        NgayGui =
                            DateTime.Now
                    };

                _context.ThongBaos.Add(
                    thongBao);
            }

            await _context.SaveChangesAsync();

            if (khachHang != null
                &&
                thongBao != null)
            {
                await GuiRealtimeAnToanAsync(
                    khachHang.MaTaiKhoan,
                    thongBao);
            }

            TempData["Success"] =
                $"Đã từ chối lịch #DL{datLich.MaDatLich}. "
                + "Lịch đã được trả về hàng chờ phân công.";

            return RedirectToAction(
                nameof(Index));
        }

        // =====================================================
        // XÁC NHẬN NHẬN LỊCH
        // =====================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> XacNhanLich(
            int maDatLich,
            string? returnTo)
        {
            int? maTaiKhoan =
                LayMaTaiKhoan();

            if (!maTaiKhoan.HasValue)
            {
                return Unauthorized();
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

            var datLich =
                await _context.DatLichs
                    .FirstOrDefaultAsync(x =>
                        x.MaDatLich == maDatLich
                        &&
                        x.MaNguoiChamSoc
                            == nguoiChamSoc.MaNguoiChamSoc);

            if (datLich == null)
            {
                TempData["Error"] =
                    "Lịch này không còn được phân công cho bạn "
                    + "hoặc đã được thay đổi.";

                return RedirectSauXuLy(
                    returnTo);
            }

            if (datLich.TrangThai
                != BookingStatus.ChoXacNhan)
            {
                TempData["Error"] =
                    "Lịch này không còn ở trạng thái "
                    + "chờ xác nhận.";

                return RedirectSauXuLy(
                    returnTo);
            }

            if (!datLich.NgayChamSoc.HasValue
                ||
                !datLich.GioBatDau.HasValue
                ||
                !datLich.GioKetThuc.HasValue)
            {
                TempData["Error"] =
                    "Lịch chưa có đầy đủ ngày hoặc "
                    + "khung giờ chăm sóc.";

                return RedirectSauXuLy(
                    returnTo);
            }

            DateTime ngayChamSoc =
                datLich.NgayChamSoc.Value.Date;

            TimeSpan gioBatDau =
                datLich.GioBatDau.Value;

            TimeSpan gioKetThuc =
                datLich.GioKetThuc.Value;

            if (gioKetThuc <= gioBatDau)
            {
                TempData["Error"] =
                    "Dữ liệu lịch không hợp lệ: "
                    + "giờ kết thúc phải lớn hơn giờ bắt đầu.";

                return RedirectSauXuLy(
                    returnTo);
            }

            DateTime thoiDiemBatDau =
                ngayChamSoc.Add(
                    gioBatDau);

            if (thoiDiemBatDau <= DateTime.Now)
            {
                TempData["Error"] =
                    "Không thể nhận lịch vì thời gian "
                    + "chăm sóc đã bắt đầu hoặc đã qua.";

                return RedirectSauXuLy(
                    returnTo);
            }

            bool trungLich =
                await _context.DatLichs
                    .AsNoTracking()
                    .AnyAsync(x =>
                        x.MaDatLich != datLich.MaDatLich
                        &&
                        x.MaNguoiChamSoc
                            == nguoiChamSoc.MaNguoiChamSoc
                        &&
                        x.NgayChamSoc.HasValue
                        &&
                        x.NgayChamSoc.Value.Date
                            == ngayChamSoc
                        &&
                        x.GioBatDau.HasValue
                        &&
                        x.GioKetThuc.HasValue
                        &&
                        (
                            x.TrangThai
                                == BookingStatus.DaXacNhan
                            ||
                            x.TrangThai
                                == BookingStatus.DangThucHien
                        )
                        &&
                        x.GioBatDau.Value < gioKetThuc
                        &&
                        x.GioKetThuc.Value > gioBatDau);

            if (trungLich)
            {
                TempData["Error"] =
                    "Không thể nhận lịch vì bạn đã có "
                    + "một lịch được xác nhận bị trùng "
                    + "khung giờ này.";

                return RedirectSauXuLy(
                    returnTo);
            }

            datLich.TrangThai =
                BookingStatus.DaXacNhan;

            var khachHang =
                await _context.KhachHangs
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x =>
                        x.MaKhachHang
                            == datLich.MaKhachHang);

            ThongBao? thongBao =
                null;

            if (khachHang != null)
            {
                thongBao =
                    new ThongBao
                    {
                        MaTaiKhoan =
                            khachHang.MaTaiKhoan,

                        TieuDe =
                            "Người chăm sóc đã nhận lịch",

                        NoiDung =
                            $"Lịch #DL{datLich.MaDatLich} "
                            + $"đã được "
                            + $"{nguoiChamSoc.HoTen ?? "người chăm sóc"} "
                            + $"xác nhận nhận lịch. "
                            + $"Thời gian: "
                            + $"{ngayChamSoc:dd/MM/yyyy} "
                            + $"{gioBatDau:hh\\:mm} - "
                            + $"{gioKetThuc:hh\\:mm}.",

                        DaDoc =
                            false,

                        NgayGui =
                            DateTime.Now
                    };

                _context.ThongBaos.Add(
                    thongBao);
            }

            await _context.SaveChangesAsync();

            if (khachHang != null
                &&
                thongBao != null)
            {
                await GuiRealtimeAnToanAsync(
                    khachHang.MaTaiKhoan,
                    thongBao);
            }

            TempData["Success"] =
                $"Đã xác nhận nhận lịch "
                + $"#DL{datLich.MaDatLich}.";

            return RedirectSauXuLy(
                returnTo);
        }

        // =====================================================
        // CẬP NHẬT TRẠNG THÁI LỊCH
        // CHECK-IN / CHECK-OUT + THÔNG BÁO REAL-TIME
        // =====================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CapNhatTrangThai(
            CaregiverScheduleStatusUpdateViewModel model)
        {
            int? maTaiKhoan =
                LayMaTaiKhoan();

            if (!maTaiKhoan.HasValue)
            {
                return Unauthorized();
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

            var datLich =
                await _context.DatLichs
                    .FirstOrDefaultAsync(x =>
                        x.MaDatLich == model.MaDatLich
                        &&
                        x.MaNguoiChamSoc
                            == nguoiChamSoc.MaNguoiChamSoc);

            if (datLich == null)
            {
                TempData["Error"] =
                    "Không tìm thấy lịch cần cập nhật.";

                return RedirectToAction(
                    nameof(Index));
            }

            if (datLich.TrangThai
                == BookingStatus.DaHuy)
            {
                TempData["Error"] =
                    "Lịch đã bị khách hàng hủy "
                    + "và không thể tiếp tục thực hiện.";

                return RedirectToAction(
                    nameof(Index));
            }

            string trangThaiMoi =
                model.TrangThai?.Trim()
                ?? string.Empty;

            if (trangThaiMoi
                == BookingStatus.DaXacNhan)
            {
                return await XacNhanLich(
                    model.MaDatLich,
                    null);
            }

            bool hopLe =
                KiemTraChuyenTrangThai(
                    datLich.TrangThai,
                    trangThaiMoi);

            if (!hopLe)
            {
                TempData["Error"] =
                    "Không thể chuyển lịch từ trạng thái "
                    + $"“{datLich.TrangThai}” sang "
                    + $"“{trangThaiMoi}”.";

                return RedirectToAction(
                    nameof(Index));
            }

            DateTime thoiDiemHienTai =
                DateTime.Now;

            datLich.TrangThai =
                trangThaiMoi;

            if (trangThaiMoi
                == BookingStatus.DangThucHien)
            {
                if (!datLich.ThoiDiemCheckIn.HasValue)
                {
                    datLich.ThoiDiemCheckIn =
                        thoiDiemHienTai;
                }
            }

            if (trangThaiMoi
                == BookingStatus.DaHoanThanh)
            {
                if (!datLich.ThoiDiemCheckOut.HasValue)
                {
                    datLich.ThoiDiemCheckOut =
                        thoiDiemHienTai;
                }
            }

            var khachHang =
                await _context.KhachHangs
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x =>
                        x.MaKhachHang
                            == datLich.MaKhachHang);

            ThongBao? thongBao =
                null;

            if (khachHang != null
                &&
                trangThaiMoi
                    == BookingStatus.DangThucHien)
            {
                thongBao =
                    new ThongBao
                    {
                        MaTaiKhoan =
                            khachHang.MaTaiKhoan,

                        TieuDe =
                            "Người chăm sóc đã check-in",

                        NoiDung =
                            $"{nguoiChamSoc.HoTen ?? "Người chăm sóc"} "
                            + $"đã bắt đầu lịch "
                            + $"#DL{datLich.MaDatLich} "
                            + $"lúc {thoiDiemHienTai:HH:mm dd/MM/yyyy}.",

                        DaDoc =
                            false,

                        NgayGui =
                            thoiDiemHienTai
                    };

                _context.ThongBaos.Add(
                    thongBao);
            }
            else if (khachHang != null
                &&
                trangThaiMoi
                    == BookingStatus.DaHoanThanh)
            {
                thongBao =
                    new ThongBao
                    {
                        MaTaiKhoan =
                            khachHang.MaTaiKhoan,

                        TieuDe =
                            "Người chăm sóc đã check-out",

                        NoiDung =
                            $"Lịch #DL{datLich.MaDatLich} "
                            + $"đã kết thúc lúc "
                            + $"{thoiDiemHienTai:HH:mm dd/MM/yyyy}. "
                            + "Bạn có thể xem nhật ký chăm sóc.",

                        DaDoc =
                            false,

                        NgayGui =
                            thoiDiemHienTai
                    };

                _context.ThongBaos.Add(
                    thongBao);
            }

            await _context.SaveChangesAsync();

            if (khachHang != null
                &&
                thongBao != null)
            {
                await GuiRealtimeAnToanAsync(
                    khachHang.MaTaiKhoan,
                    thongBao);
            }

            TempData["Success"] =
                $"Đã cập nhật lịch #{datLich.MaDatLich} "
                + $"sang trạng thái “{trangThaiMoi}”.";

            return RedirectToAction(
                nameof(Index));
        }

        // =====================================================
        // KIỂM TRA LUỒNG TRẠNG THÁI
        // =====================================================
        private static bool KiemTraChuyenTrangThai(
            string? trangThaiHienTai,
            string trangThaiMoi)
        {
            return trangThaiHienTai switch
            {
                BookingStatus.ChoXacNhan =>
                    trangThaiMoi
                        == BookingStatus.DaXacNhan,

                BookingStatus.DaXacNhan =>
                    trangThaiMoi
                        == BookingStatus.DangThucHien,

                BookingStatus.DangThucHien =>
                    trangThaiMoi
                        == BookingStatus.DaHoanThanh,

                _ => false
            };
        }

        // =====================================================
        // GỬI THÔNG BÁO REAL-TIME AN TOÀN
        // Không để lỗi SignalR làm hỏng nghiệp vụ đã lưu DB.
        // =====================================================
        private async Task GuiRealtimeAnToanAsync(
            int maTaiKhoan,
            ThongBao thongBao)
        {
            try
            {
                await _hubContext
                    .Clients
                    .User(
                        maTaiKhoan.ToString())
                    .SendAsync(
                        "ReceiveNotification",
                        new
                        {
                            maThongBao =
                                thongBao.MaThongBao,

                            tieuDe =
                                thongBao.TieuDe,

                            noiDung =
                                thongBao.NoiDung,

                            ngayGui =
                                thongBao.NgayGui
                        });
            }
            catch (Exception ex)
            {
                _logger.LogWarning(
                    ex,
                    "Không thể gửi thông báo SignalR cho tài khoản {MaTaiKhoan}.",
                    maTaiKhoan);
            }
        }

        // =====================================================
        // ĐIỀU HƯỚNG SAU XỬ LÝ
        // =====================================================
        private IActionResult RedirectSauXuLy(
            string? returnTo)
        {
            if (string.Equals(
                    returnTo,
                    "dashboard",
                    StringComparison.OrdinalIgnoreCase))
            {
                return RedirectToAction(
                    "Index",
                    "Dashboard",
                    new
                    {
                        area = "Caregiver"
                    });
            }

            return RedirectToAction(
                nameof(Index));
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
    }
}
