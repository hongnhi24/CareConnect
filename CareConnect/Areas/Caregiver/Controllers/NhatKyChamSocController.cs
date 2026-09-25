using CareConnect.Constants;
using CareConnect.Data;
using CareConnect.Models;
using CareConnect.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace CareConnect.Areas.Caregiver.Controllers
{
    [Area("Caregiver")]
    [Authorize(Roles = "Người chăm sóc")]
    public class NhatKyChamSocController : Controller
    {
        private readonly CareConnectDbContext _context;

        public NhatKyChamSocController(
            CareConnectDbContext context)
        {
            _context = context;
        }

        // =====================================================
        // DANH SÁCH NHẬT KÝ CHĂM SÓC
        // =====================================================
        [HttpGet]
        public async Task<IActionResult> Index(
            string? tuKhoa,
            string? trangThaiNhatKy,
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

            string trangThaiDaChuanHoa =
                trangThaiNhatKy?.Trim().ToLower()
                ?? string.Empty;

            /*
             * Chỉ hiển thị lịch đang thực hiện hoặc đã hoàn thành.
             * Đây là những lịch phù hợp để lập nhật ký chăm sóc.
             */
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

                join nhatKy in
                    _context.NhatKyChamSocs.AsNoTracking()

                    on datLich.MaDatLich
                    equals nhatKy.MaDatLich

                    into nhatKyGroup

                from nhatKy in
                    nhatKyGroup.DefaultIfEmpty()

                where datLich.MaNguoiChamSoc
                        == maNguoiChamSoc
                    &&
                    (
                        datLich.TrangThai ==
    BookingStatus.DangThucHien
||
datLich.TrangThai ==
    BookingStatus.DaHoanThanh
                    )

                select new
                {
                    DatLich = datLich,
                    KhachHang = khachHang,
                    BenhNhan = benhNhan,
                    DichVu = dichVu,
                    NhatKy = nhatKy
                };

            if (!string.IsNullOrWhiteSpace(
                    tuKhoaDaChuanHoa))
            {
                query =
                    query.Where(x =>
                        x.BenhNhan.HoTen.Contains(
                            tuKhoaDaChuanHoa)
                        ||
                        x.KhachHang.HoTen.Contains(
                            tuKhoaDaChuanHoa)
                        ||
                        x.DichVu.TenDichVu.Contains(
                            tuKhoaDaChuanHoa)
                        ||
                        x.DatLich.MaDatLich
                            .ToString()
                            .Contains(tuKhoaDaChuanHoa));
            }

            if (trangThaiDaChuanHoa == "da-ghi")
            {
                query =
                    query.Where(x =>
                        x.NhatKy != null);
            }
            else if (trangThaiDaChuanHoa == "chua-ghi")
            {
                query =
                    query.Where(x =>
                        x.NhatKy == null);
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

            /*
             * Nhật ký vừa cập nhật được ưu tiên lên đầu.
             * Sau đó mới đến lịch chăm sóc gần đây nhất.
             */
            var danhSach =
                await query
                    .OrderByDescending(x =>
                        x.NhatKy != null
                            ? x.NhatKy.NgayCapNhat
                            : x.DatLich.NgayChamSoc)
                    .ThenByDescending(x =>
                        x.DatLich.GioBatDau)
                    .Select(x =>
                        new CaregiverCareJournalItemViewModel
                        {
                            MaDatLich =
                                x.DatLich.MaDatLich,

                            MaNhatKy =
                                x.NhatKy != null
                                    ? x.NhatKy.MaNhatKy
                                    : null,

                            TenBenhNhan =
                                x.BenhNhan.HoTen,

                            TenKhachHang =
                                x.KhachHang.HoTen,

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

                            TrangThaiLich =
                                x.DatLich.TrangThai
                                ?? "Chưa xác định",

                            TinhTrangSucKhoe =
                                x.NhatKy != null
                                    ? x.NhatKy.TinhTrangSucKhoe
                                    : null,

                            HuyetAp =
                                x.NhatKy != null
                                    ? x.NhatKy.HuyetAp
                                    : null,

                            NhipTim =
                                x.NhatKy != null
                                    ? x.NhatKy.NhipTim
                                    : null,

                            NhietDo =
                                x.NhatKy != null
                                    ? x.NhatKy.NhietDo
                                    : null,

                            CanNang =
                                x.NhatKy != null
                                    ? x.NhatKy.CanNang
                                    : null,

                            TinhTrangAnUong =
                                x.NhatKy != null
                                    ? x.NhatKy.TinhTrangAnUong
                                    : null,

                            ThuocDaUong =
                                x.NhatKy != null
                                    ? x.NhatKy.ThuocDaUong
                                    : null,

                            GhiChu =
                                x.NhatKy != null
                                    ? x.NhatKy.GhiChu
                                    : null,

                            NgayCapNhat =
                                x.NhatKy != null
                                    ? x.NhatKy.NgayCapNhat
                                    : null
                        })
                    .ToListAsync();

            var tatCaLichCoTheGhi =
                _context.DatLichs
                    .AsNoTracking()
                    .Where(x =>
                        x.MaNguoiChamSoc
                            == maNguoiChamSoc
                        &&
                        (
                            x.TrangThai
                                == BookingStatus.DangThucHien
                            ||
                            x.TrangThai
                                == BookingStatus.DaHoanThanh
                        ));

            int tongLichCoTheGhi =
                await tatCaLichCoTheGhi.CountAsync();

            int daGhiNhatKy =
                await (
                    from datLich in
                        tatCaLichCoTheGhi

                    join nhatKy in
                        _context.NhatKyChamSocs
                            .AsNoTracking()

                        on datLich.MaDatLich
                        equals nhatKy.MaDatLich

                    select nhatKy
                ).CountAsync();

            int chuaGhiNhatKy =
                Math.Max(
                    tongLichCoTheGhi - daGhiNhatKy,
                    0);

            DateTime homNay =
                DateTime.Today;

            int capNhatHomNay =
                await (
                    from datLich in
                        tatCaLichCoTheGhi

                    join nhatKy in
                        _context.NhatKyChamSocs
                            .AsNoTracking()

                        on datLich.MaDatLich
                        equals nhatKy.MaDatLich

                    where nhatKy.NgayCapNhat.HasValue
                        &&
                        nhatKy.NgayCapNhat.Value.Date
                            == homNay

                    select nhatKy
                ).CountAsync();

            var model =
                new CaregiverCareJournalViewModel
                {
                    TongLichCoTheGhi =
                        tongLichCoTheGhi,

                    DaGhiNhatKy =
                        daGhiNhatKy,

                    ChuaGhiNhatKy =
                        chuaGhiNhatKy,

                    CapNhatHomNay =
                        capNhatHomNay,

                    TuKhoa =
                        tuKhoaDaChuanHoa,

                    TrangThaiNhatKy =
                        trangThaiDaChuanHoa,

                    TuNgay =
                        tuNgay,

                    DenNgay =
                        denNgay,

                    DanhSach =
                        danhSach
                };

            return View(model);
        }

        // =====================================================
        // TẠO HOẶC CẬP NHẬT NHẬT KÝ
        // =====================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Luu(
            CaregiverCareJournalSaveViewModel model)
        {
            int? maTaiKhoan = LayMaTaiKhoan();

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
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x =>
                        x.MaDatLich == model.MaDatLich
                        &&
                        x.MaNguoiChamSoc
                            == nguoiChamSoc.MaNguoiChamSoc);

            if (datLich == null)
            {
                TempData["Error"] =
                    "Không tìm thấy lịch chăm sóc cần cập nhật.";

                return RedirectToAction(nameof(Index));
            }

            bool trangThaiHopLe =
                datLich.TrangThai ==
    BookingStatus.DangThucHien
||
datLich.TrangThai ==
    BookingStatus.DaHoanThanh;

            if (!trangThaiHopLe)
            {
                TempData["Error"] =
                    "Chỉ có thể ghi nhật ký cho lịch đang thực hiện "
                    + "hoặc đã hoàn thành.";

                return RedirectToAction(nameof(Index));
            }

            if (!ModelState.IsValid)
            {
                string thongBaoLoi =
                    string.Join(
                        " | ",
                        ModelState.Values
                            .SelectMany(x => x.Errors)
                            .Select(x =>
                                string.IsNullOrWhiteSpace(
                                    x.ErrorMessage)
                                    ? "Thông tin chưa hợp lệ."
                                    : x.ErrorMessage));

                TempData["Error"] =
                    string.IsNullOrWhiteSpace(thongBaoLoi)
                        ? "Vui lòng kiểm tra lại nhật ký."
                        : thongBaoLoi;

                return RedirectToAction(nameof(Index));
            }

            var nhatKy =
                await _context.NhatKyChamSocs
                    .FirstOrDefaultAsync(x =>
                        x.MaDatLich == model.MaDatLich);

            bool laTaoMoi =
                nhatKy == null;

            if (laTaoMoi)
            {
                nhatKy =
                    new NhatKyChamSoc
                    {
                        MaDatLich =
                            model.MaDatLich
                    };

                _context.NhatKyChamSocs.Add(nhatKy);
            }

            nhatKy!.TinhTrangSucKhoe =
                string.IsNullOrWhiteSpace(
                    model.TinhTrangSucKhoe)
                    ? null
                    : model.TinhTrangSucKhoe.Trim();

            nhatKy.HuyetAp =
                string.IsNullOrWhiteSpace(model.HuyetAp)
                    ? null
                    : model.HuyetAp.Trim();

            nhatKy.NhipTim =
                model.NhipTim;

            nhatKy.NhietDo =
                model.NhietDo;

            nhatKy.CanNang =
                model.CanNang;

            nhatKy.TinhTrangAnUong =
                string.IsNullOrWhiteSpace(
                    model.TinhTrangAnUong)
                    ? null
                    : model.TinhTrangAnUong.Trim();

            nhatKy.ThuocDaUong =
                string.IsNullOrWhiteSpace(
                    model.ThuocDaUong)
                    ? null
                    : model.ThuocDaUong.Trim();

            nhatKy.GhiChu =
                string.IsNullOrWhiteSpace(model.GhiChu)
                    ? null
                    : model.GhiChu.Trim();

            nhatKy.NgayCapNhat =
                DateTime.Now;

            try
            {
                await _context.SaveChangesAsync();

                TempData["Success"] =
                    laTaoMoi
                        ? "Nhật ký chăm sóc đã được tạo thành công."
                        : "Nhật ký chăm sóc đã được cập nhật thành công.";
            }
            catch (DbUpdateException)
            {
                TempData["Error"] =
                    "Không thể lưu nhật ký vào cơ sở dữ liệu.";
            }

            return RedirectToAction(nameof(Index));
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