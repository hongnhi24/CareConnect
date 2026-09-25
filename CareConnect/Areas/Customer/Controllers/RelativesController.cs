

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
    public class RelativesController : Controller
    {
        private readonly CareConnectDbContext _context;

        public RelativesController(
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

                where
                    taiKhoan.TenDangNhap == tenDangNhap

                select (int?)khachHang.MaKhachHang
            )
            .FirstOrDefaultAsync();
        }

        private async Task<string> TaoMaBenhNhanCode()
        {
            string tienTo =
                $"BN{DateTime.Now:yyyyMMdd}";

            int soLuongTrongNgay =
                await _context.BenhNhans
                    .AsNoTracking()
                    .CountAsync(x =>
                        x.MaBenhNhanCode != null
                        &&
                        x.MaBenhNhanCode.StartsWith(tienTo));

            return $"{tienTo}{soLuongTrongNgay + 1:D3}";
        }

        [HttpGet]
        public async Task<IActionResult> Index(
            string? tuKhoa)
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

            tuKhoa =
                tuKhoa?.Trim()
                ?? string.Empty;

            var query =
     _context.BenhNhans
         .AsNoTracking()
         .Where(x =>
             x.MaKhachHang
                 == maKhachHang.Value
             &&
             x.TrangThaiHoatDong);

            if (!string.IsNullOrWhiteSpace(tuKhoa))
            {
                query = query.Where(x =>
                    x.HoTen.Contains(tuKhoa)
                    ||
                    (
                        x.MaBenhNhanCode != null
                        &&
                        x.MaBenhNhanCode.Contains(tuKhoa)
                    )
                    ||
                    (
                        x.SoDienThoai != null
                        &&
                        x.SoDienThoai.Contains(tuKhoa)
                    )
                    ||
                    (
                        x.QuanHe != null
                        &&
                        x.QuanHe.Contains(tuKhoa)
                    )
                    ||
                    (
                        x.DiaChi != null
                        &&
                        x.DiaChi.Contains(tuKhoa)
                    )
                    ||
                    (
                        x.TinhTrangSucKhoe != null
                        &&
                        x.TinhTrangSucKhoe.Contains(tuKhoa)
                    )
                    ||
                    (
                        x.TienSuBenh != null
                        &&
                        x.TienSuBenh.Contains(tuKhoa)
                    )
                    ||
                    (
                        x.DiUngThuoc != null
                        &&
                        x.DiUngThuoc.Contains(tuKhoa)
                    ));
            }

            var danhSach =
                await query
                    .OrderByDescending(x =>
                        x.NgayTao)
                    .ThenBy(x =>
                        x.HoTen)
                    .Select(x =>
                        new CustomerRelativeItemViewModel
                        {
                            MaBenhNhan =
                                x.MaBenhNhan,

                            MaBenhNhanCode =
                                x.MaBenhNhanCode
                                ?? $"BN{x.MaBenhNhan:D4}",

                            HoTen =
                                x.HoTen,

                            GioiTinh =
                                x.GioiTinh
                                ?? "Chưa cập nhật",

                            NgaySinh =
                                x.NgaySinh,

                            SoDienThoai =
                                x.SoDienThoai
                                ?? "Chưa cập nhật",

                            DiaChi =
                                x.DiaChi
                                ?? "Chưa cập nhật",

                            TinhTrangSucKhoe =
                                x.TinhTrangSucKhoe
                                ?? "Chưa cập nhật",

                            TienSuBenh =
                                x.TienSuBenh
                                ?? "Chưa cập nhật",

                            NhomMau =
                                x.NhomMau
                                ?? "Chưa cập nhật",

                            DiUngThuoc =
                                x.DiUngThuoc
                                ?? "Không ghi nhận",

                            QuanHe =
                                x.QuanHe
                                ?? "Người thân",

                            GhiChu =
                                x.GhiChu
                        })
                    .ToListAsync();

            var model =
                new CustomerRelativeListViewModel
                {
                    TongHoSo =
                        danhSach.Count,

                    TuKhoa =
                        tuKhoa,

                    DanhSachNguoiThan =
                        danhSach
                };

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            int? maKhachHang =
                await LayMaKhachHangDangNhap();

            if (!maKhachHang.HasValue)
            {
                return Forbid();
            }

            return View(
                new CustomerRelativeFormViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            CustomerRelativeFormViewModel model)
        {
            int? maKhachHang =
                await LayMaKhachHangDangNhap();

            if (!maKhachHang.HasValue)
            {
                return Forbid();
            }

            KiemTraDuLieu(model);

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            string maBenhNhanCode =
                await TaoMaBenhNhanCode();

            var benhNhan =
                new BenhNhan
                {
                    MaKhachHang =
                        maKhachHang.Value,

                    HoTen =
                        model.HoTen.Trim(),

                    GioiTinh =
                        ChuanHoa(model.GioiTinh),

                    NgaySinh =
                        model.NgaySinh,

                    SoDienThoai =
                        ChuanHoa(model.SoDienThoai),

                    DiaChi =
                        ChuanHoa(model.DiaChi),

                    TinhTrangSucKhoe =
                        ChuanHoa(
                            model.TinhTrangSucKhoe),

                    TienSuBenh =
                        ChuanHoa(model.TienSuBenh),

                    GhiChu =
                        ChuanHoa(model.GhiChu),

                    NhomMau =
                        ChuanHoa(model.NhomMau),

                    DiUngThuoc =
                        ChuanHoa(model.DiUngThuoc),

                    NguoiLienHeKhanCap =
                        ChuanHoa(
                            model.NguoiLienHeKhanCap),

                    SoDienThoaiKhanCap =
                        ChuanHoa(
                            model.SoDienThoaiKhanCap),

                    QuanHe =
                        ChuanHoa(model.QuanHe),

                    NgayTao =
                        DateTime.Now,

                    MaBenhNhanCode =
                        maBenhNhanCode
                };

            _context.BenhNhans.Add(benhNhan);

            await _context.SaveChangesAsync();

            TempData["Success"] =
                "Đã thêm hồ sơ người thân thành công.";

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
                await _context.BenhNhans
                    .AsNoTracking()
                    .Where(x =>
    x.MaBenhNhan == id
    &&
    x.MaKhachHang
        == maKhachHang.Value
    &&
    x.TrangThaiHoatDong)
                    .Select(x =>
                        new CustomerRelativeDetailsViewModel
                        {
                            MaBenhNhan =
                                x.MaBenhNhan,

                            MaBenhNhanCode =
                                x.MaBenhNhanCode,

                            HoTen =
                                x.HoTen,

                            GioiTinh =
                                x.GioiTinh,

                            NgaySinh =
                                x.NgaySinh,

                            SoDienThoai =
                                x.SoDienThoai,

                            DiaChi =
                                x.DiaChi,

                            TinhTrangSucKhoe =
                                x.TinhTrangSucKhoe,

                            TienSuBenh =
                                x.TienSuBenh,

                            GhiChu =
                                x.GhiChu,

                            NhomMau =
                                x.NhomMau,

                            DiUngThuoc =
                                x.DiUngThuoc,

                            NguoiLienHeKhanCap =
                                x.NguoiLienHeKhanCap,

                            SoDienThoaiKhanCap =
                                x.SoDienThoaiKhanCap,

                            QuanHe =
                                x.QuanHe,

                            NgayTao =
                                x.NgayTao,

                            SoLichChamSoc =
                                _context.DatLichs.Count(
                                    lich =>
                                        lich.MaBenhNhan
                                            == x.MaBenhNhan)
                        })
                    .FirstOrDefaultAsync();

            if (model == null)
            {
                TempData["Error"] =
            "Không tìm thấy hồ sơ người thân.";

                return RedirectToAction(nameof(Index));
            }

            return View(model);
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
                await _context.BenhNhans
                    .AsNoTracking()
                    .Where(x =>
                        x.MaBenhNhan == id
                        &&
                        x.MaKhachHang
                            == maKhachHang.Value)
                    .Select(x =>
                        new CustomerRelativeFormViewModel
                        {
                            MaBenhNhan =
                                x.MaBenhNhan,

                            MaBenhNhanCode =
                                x.MaBenhNhanCode,

                            HoTen =
                                x.HoTen,

                            GioiTinh =
                                x.GioiTinh,

                            NgaySinh =
                                x.NgaySinh,

                            SoDienThoai =
                                x.SoDienThoai,

                            DiaChi =
                                x.DiaChi,

                            TinhTrangSucKhoe =
                                x.TinhTrangSucKhoe,

                            TienSuBenh =
                                x.TienSuBenh,

                            GhiChu =
                                x.GhiChu,

                            NhomMau =
                                x.NhomMau,

                            DiUngThuoc =
                                x.DiUngThuoc,

                            NguoiLienHeKhanCap =
                                x.NguoiLienHeKhanCap,

                            SoDienThoaiKhanCap =
                                x.SoDienThoaiKhanCap,

                            QuanHe =
                                x.QuanHe
                        })
                    .FirstOrDefaultAsync();

            if (model == null)
            {
                TempData["Error"] =
            "Không tìm thấy hồ sơ người thân.";

                return RedirectToAction(nameof(Index));
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            CustomerRelativeFormViewModel model)
        {
            int? maKhachHang =
                await LayMaKhachHangDangNhap();

            if (!maKhachHang.HasValue)
            {
                return Forbid();
            }

            KiemTraDuLieu(model);

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var benhNhan =
    await _context.BenhNhans
        .FirstOrDefaultAsync(x =>
            x.MaBenhNhan
                == model.MaBenhNhan
            &&
            x.MaKhachHang
                == maKhachHang.Value
            &&
            x.TrangThaiHoatDong);

            if (benhNhan == null)
            {
                TempData["Error"] =
          "Không tìm thấy hồ sơ hoặc bạn không có quyền chỉnh sửa.";

                return RedirectToAction(nameof(Index));
            }

            benhNhan.HoTen =
                model.HoTen.Trim();

            benhNhan.GioiTinh =
                ChuanHoa(model.GioiTinh);

            benhNhan.NgaySinh =
                model.NgaySinh;

            benhNhan.SoDienThoai =
                ChuanHoa(model.SoDienThoai);

            benhNhan.DiaChi =
                ChuanHoa(model.DiaChi);

            benhNhan.TinhTrangSucKhoe =
                ChuanHoa(
                    model.TinhTrangSucKhoe);

            benhNhan.TienSuBenh =
                ChuanHoa(model.TienSuBenh);

            benhNhan.GhiChu =
                ChuanHoa(model.GhiChu);

            benhNhan.NhomMau =
                ChuanHoa(model.NhomMau);

            benhNhan.DiUngThuoc =
                ChuanHoa(model.DiUngThuoc);

            benhNhan.NguoiLienHeKhanCap =
                ChuanHoa(
                    model.NguoiLienHeKhanCap);

            benhNhan.SoDienThoaiKhanCap =
                ChuanHoa(
                    model.SoDienThoaiKhanCap);

            benhNhan.QuanHe =
                ChuanHoa(model.QuanHe);

            await _context.SaveChangesAsync();

            TempData["Success"] =
                "Đã cập nhật hồ sơ người thân thành công.";

            return RedirectToAction(
                nameof(Details),
                new
                {
                    id = model.MaBenhNhan
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

            var benhNhan =
                await _context.BenhNhans
                    .FirstOrDefaultAsync(x =>
                        x.MaBenhNhan == id
                        &&
                        x.MaKhachHang
                            == maKhachHang.Value
                        &&
                        x.TrangThaiHoatDong);

            if (benhNhan == null)
            {
                TempData["Error"] =
                    "Không tìm thấy hồ sơ người thân.";

                return RedirectToAction(nameof(Index));
            }

            /*
             * Không cho xóa hồ sơ nếu đang có
             * một ca chăm sóc thực tế đang diễn ra.
             */
            bool coLichDangThucHien =
                await _context.DatLichs
                    .AnyAsync(x =>
                        x.MaBenhNhan == id
                        &&
                        x.TrangThai ==
                            "Đang thực hiện");

            if (coLichDangThucHien)
            {
                TempData["Error"] =
                    "Không thể xóa hồ sơ vì đang có "
                    + "lịch chăm sóc đang thực hiện.";

                return RedirectToAction(nameof(Index));
            }

            await using var transaction =
                await _context.Database
                    .BeginTransactionAsync();

            try
            {
                /*
                 * Các lịch chưa diễn ra hoặc đang chờ
                 * sẽ được hủy.
                 *
                 * Không đụng vào lịch Đã hoàn thành
                 * vì đây là lịch sử nghiệp vụ.
                 */
                var lichCanHuy =
                    await _context.DatLichs
                        .Where(x =>
                            x.MaBenhNhan == id
                            &&
                            (
                                x.TrangThai ==
                                    "Chờ xác nhận"
                                ||
                                x.TrangThai ==
                                    "Đã xác nhận"
                            ))
                        .ToListAsync();

                foreach (var lich in lichCanHuy)
                {
                    lich.TrangThai =
                        "Đã hủy";

                    lich.LyDoHuy =
                        "Hồ sơ người thân đã được xóa.";

                    lich.NguoiHuy =
                        "Khách hàng";

                    lich.NgayHuy =
                        DateTime.Now;
                }

                /*
                 * Soft delete hồ sơ.
                 */
                benhNhan.TrangThaiHoatDong =
                    false;

                benhNhan.NgayXoa =
                    DateTime.Now;

                await _context.SaveChangesAsync();

                await transaction.CommitAsync();

                TempData["Success"] =
                    "Đã xóa hồ sơ người thân. "
                    + "Các lịch chưa hoàn thành liên quan "
                    + "đã được vô hiệu hóa.";
            }
            catch
            {
                await transaction.RollbackAsync();

                TempData["Error"] =
                    "Không thể xóa hồ sơ người thân. "
                    + "Vui lòng thử lại.";
            }

            return RedirectToAction(nameof(Index));
        }
        private void KiemTraDuLieu(
            CustomerRelativeFormViewModel model)
        {
            if (model.NgaySinh.HasValue
                &&
                model.NgaySinh.Value.Date
                    > DateTime.Today)
            {
                ModelState.AddModelError(
                    nameof(model.NgaySinh),
                    "Ngày sinh không được lớn hơn ngày hiện tại.");
            }

            if (!string.IsNullOrWhiteSpace(
                    model.SoDienThoai)
                &&
                model.SoDienThoai.Trim().Length < 9)
            {
                ModelState.AddModelError(
                    nameof(model.SoDienThoai),
                    "Số điện thoại phải có ít nhất 9 chữ số.");
            }

            if (!string.IsNullOrWhiteSpace(
                    model.SoDienThoaiKhanCap)
                &&
                model.SoDienThoaiKhanCap.Trim().Length < 9)
            {
                ModelState.AddModelError(
                    nameof(model.SoDienThoaiKhanCap),
                    "Số điện thoại khẩn cấp phải có ít nhất 9 chữ số.");
            }
        }

        private static string? ChuanHoa(
            string? giaTri)
        {
            return string.IsNullOrWhiteSpace(giaTri)
                ? null
                : giaTri.Trim();
        }
    }
}