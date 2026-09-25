using CareConnect.Data;
using CareConnect.Models;
using CareConnect.Services;
using CareConnect.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CareConnect.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Quản trị viên")]
    public class UsersController : Controller
    {
        private readonly CareConnectDbContext _context;
        private readonly IPasswordSecurityService
            _passwordSecurity;

        public UsersController(
            CareConnectDbContext context,
            IPasswordSecurityService passwordSecurity)
        {
            _context = context;
            _passwordSecurity = passwordSecurity;
        }
        [HttpGet]
        public IActionResult Create()
        {
            return View(new AdminCreateUserViewModel
            {
                TrangThai = true
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
    AdminCreateUserViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            string email =
                model.Email.Trim().ToLowerInvariant();

            string tenDangNhap =
                model.TenDangNhap.Trim();

            bool emailDaTonTai =
                await _context.TaiKhoans
                    .AnyAsync(x =>
                        x.Email == email);

            if (emailDaTonTai)
            {
                ModelState.AddModelError(
                    nameof(model.Email),
                    "Email này đã tồn tại trong hệ thống.");

                return View(model);
            }

            bool tenDangNhapDaTonTai =
                await _context.TaiKhoans
                    .AnyAsync(x =>
                        x.TenDangNhap == tenDangNhap);

            if (tenDangNhapDaTonTai)
            {
                ModelState.AddModelError(
                    nameof(model.TenDangNhap),
                    "Tên đăng nhập này đã tồn tại.");

                return View(model);
            }

            var vaiTro =
                await _context.VaiTros
                    .FirstOrDefaultAsync(x =>
                        x.TenVaiTro == model.VaiTro);

            if (vaiTro == null)
            {
                ModelState.AddModelError(
                    nameof(model.VaiTro),
                    "Vai trò không hợp lệ.");

                return View(model);
            }

            await using var transaction =
                await _context.Database.BeginTransactionAsync();

            try
            {
                var taiKhoan = new TaiKhoan
                {
                    TenDangNhap = tenDangNhap,
                    Email = email,
                    MatKhau = string.Empty,
                    MaVaiTro = vaiTro.MaVaiTro,
                    TrangThai = model.TrangThai,
                    NgayTao = DateTime.Now
                };

                taiKhoan.MatKhau =
                    _passwordSecurity.HashPassword(
                        taiKhoan,
                        model.MatKhau);

                _context.TaiKhoans.Add(taiKhoan);

                await _context.SaveChangesAsync();

                if (model.VaiTro == "Khách hàng")
                {
                    var khachHang = new KhachHang
                    {
                        MaTaiKhoan = taiKhoan.MaTaiKhoan,

                        HoTen = model.HoTen.Trim(),

                        SoDienThoai = string.IsNullOrWhiteSpace(model.SoDienThoai)
                            ? null
                            : model.SoDienThoai.Trim(),

                        Email = email,

                        NgayTao = DateTime.Now,

                        GioiTinh = null,
                        NgaySinh = null,
                        DiaChi = null,
                        CCCD = null
                    };

                    _context.KhachHangs.Add(khachHang);
                }
                else if (model.VaiTro == "Người chăm sóc")
                {
                    // Tài khoản phải hoạt động để người chăm sóc có thể
                    // đăng nhập và hoàn thiện hồ sơ lần đầu.
                    taiKhoan.TrangThai = true;

                    var nguoiChamSoc = new NguoiChamSoc
                    {
                        MaTaiKhoan = taiKhoan.MaTaiKhoan,

                        HoTen = model.HoTen.Trim(),

                        SoDienThoai =
                            string.IsNullOrWhiteSpace(model.SoDienThoai)
                                ? null
                                : model.SoDienThoai.Trim(),

                        Email = email,

                        // Các thông tin nghề nghiệp chưa được nhập.
                        ChuyenMon = null,
                        KinhNghiem = null,
                        KhuVucHoatDong = null,
                        GiaTheoGio = null,
                        GioiThieu = null,

                        DanhGia = 0,

                        // Người chăm sóc sẽ bổ sung hồ sơ sau khi đăng nhập.
                        TrangThai = "Chưa hoàn thiện",

                        NgayTao = DateTime.Now
                    };

                    _context.NguoiChamSocs.Add(nguoiChamSoc);
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                TempData["Success"] =
                    $"Đã tạo tài khoản cho {model.HoTen}.";

                return RedirectToAction(nameof(Index));
            }
            catch
            {
                await transaction.RollbackAsync();

                ModelState.AddModelError(
                    string.Empty,
                    "Không thể tạo tài khoản. Vui lòng kiểm tra dữ liệu và thử lại.");

                return View(model);
            }
        }
        [HttpGet]
        public async Task<IActionResult> Index(
            string? tuKhoa,
            string? vaiTro,
            string? trangThai)
        {
            tuKhoa = tuKhoa?.Trim() ?? string.Empty;
            vaiTro = vaiTro?.Trim() ?? string.Empty;
            trangThai = trangThai?.Trim() ?? string.Empty;

            /*
             * Lấy tài khoản và vai trò trước.
             * Sau đó ghép họ tên từ KhachHang hoặc NguoiChamSoc.
             */
            var taiKhoans = await _context.TaiKhoans
    .AsNoTracking()
    .OrderByDescending(x => x.NgayTao)
    .ToListAsync();

            var vaiTros = await _context.VaiTros
                .AsNoTracking()
                .ToDictionaryAsync(
                    x => x.MaVaiTro,
                    x => x.TenVaiTro);

            var khachHangs = await _context.KhachHangs
                .AsNoTracking()
                .ToDictionaryAsync(
                    x => x.MaTaiKhoan,
                    x => x);

            var nguoiChamSocs = await _context.NguoiChamSocs
                .AsNoTracking()
                .Where(x => x.MaTaiKhoan != null)
                .ToDictionaryAsync(
                    x => x.MaTaiKhoan!.Value,
                    x => x);

            var danhSach =
                new List<AdminUserItemViewModel>();

            foreach (var taiKhoan in taiKhoans)
            {
                string tenVaiTro =
     vaiTros.TryGetValue(
         taiKhoan.MaVaiTro,
         out string? tenVaiTroTimThay)
             ? tenVaiTroTimThay
             : "Chưa xác định";

                string hoTen =
                    taiKhoan.TenDangNhap;

                int? maNguoiChamSoc = null;
                string trangThaiHoSo = string.Empty;
                string chuyenMon = string.Empty;
                string khuVucHoatDong = string.Empty;
                int kinhNghiem = 0;

                if (khachHangs.TryGetValue(
                        taiKhoan.MaTaiKhoan,
                        out var khachHang))
                {
                    hoTen =
                        string.IsNullOrWhiteSpace(khachHang.HoTen)
                            ? taiKhoan.TenDangNhap
                            : khachHang.HoTen;
                }

                if (nguoiChamSocs.TryGetValue(
                        taiKhoan.MaTaiKhoan,
                        out var nguoiChamSoc))
                {
                    hoTen =
                        string.IsNullOrWhiteSpace(
                            nguoiChamSoc.HoTen)
                            ? taiKhoan.TenDangNhap
                            : nguoiChamSoc.HoTen;

                    maNguoiChamSoc =
                        nguoiChamSoc.MaNguoiChamSoc;

                    trangThaiHoSo =
                        nguoiChamSoc.TrangThai
                        ?? string.Empty;

                    chuyenMon =
                        nguoiChamSoc.ChuyenMon
                        ?? string.Empty;

                    khuVucHoatDong =
                        nguoiChamSoc.KhuVucHoatDong
                        ?? string.Empty;

                    kinhNghiem =
                        nguoiChamSoc.KinhNghiem
                        ?? 0;
                }

                danhSach.Add(
                    new AdminUserItemViewModel
                    {
                        MaTaiKhoan =
                            taiKhoan.MaTaiKhoan,

                        MaNguoiChamSoc =
                            maNguoiChamSoc,

                        HoTen =
                            hoTen,

                        Email =
                            taiKhoan.Email
                            ?? string.Empty,

                        TenDangNhap =
                            taiKhoan.TenDangNhap,

                        TenVaiTro =
                            tenVaiTro,

                        TaiKhoanHoatDong =
                            taiKhoan.TrangThai,

                        TrangThaiHoSo =
                            trangThaiHoSo,

                        NgayTao =
                            taiKhoan.NgayTao,

                        ChuyenMon =
                            chuyenMon,

                        KhuVucHoatDong =
                            khuVucHoatDong,

                        KinhNghiem =
                            kinhNghiem
                    });
            }

            /*
             * Không hiển thị tài khoản quản trị viên trong danh sách
             * để tránh admin tự khóa tài khoản đang đăng nhập.
             */
            danhSach = danhSach
                .Where(x =>
                    x.TenVaiTro != "Quản trị viên")
                .ToList();

            if (!string.IsNullOrWhiteSpace(tuKhoa))
            {
                danhSach = danhSach
                    .Where(x =>
                        x.HoTen.Contains(
                            tuKhoa,
                            StringComparison.OrdinalIgnoreCase)
                        ||
                        x.Email.Contains(
                            tuKhoa,
                            StringComparison.OrdinalIgnoreCase)
                        ||
                        x.TenDangNhap.Contains(
                            tuKhoa,
                            StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }

            if (!string.IsNullOrWhiteSpace(vaiTro))
            {
                danhSach = danhSach
                    .Where(x =>
                        x.TenVaiTro == vaiTro)
                    .ToList();
            }

            if (!string.IsNullOrWhiteSpace(trangThai))
            {
                danhSach = danhSach
                    .Where(x =>
                        x.TrangThaiHienThi == trangThai)
                    .ToList();
            }

            int tongKhachHang =
                await _context.KhachHangs
                    .AsNoTracking()
                    .CountAsync();

            int tongNguoiChamSoc =
                await _context.NguoiChamSocs
                    .AsNoTracking()
                    .CountAsync();

            int soChoDuyet =
                await _context.NguoiChamSocs
                    .AsNoTracking()
                    .CountAsync(x =>
                        x.TrangThai == "Chờ duyệt");

            int soTaiKhoanBiKhoa =
                await _context.TaiKhoans
                    .AsNoTracking()
                    .CountAsync(x =>
                        !x.TrangThai);

            var model =
                new AdminUserManagementViewModel
                {
                    TongNguoiDung =
                        tongKhachHang +
                        tongNguoiChamSoc,

                    TongKhachHang =
                        tongKhachHang,

                    TongNguoiChamSoc =
                        tongNguoiChamSoc,

                    SoNguoiChamSocChoDuyet =
                        soChoDuyet,

                    SoTaiKhoanBiKhoa =
                        soTaiKhoanBiKhoa,

                    TuKhoa =
                        tuKhoa,

                    VaiTro =
                        vaiTro,

                    TrangThai =
                        trangThai,

                    DanhSachNguoiDung =
                        danhSach
                };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> KhoaTaiKhoan(
            int maTaiKhoan)
        {
            var taiKhoan =
                await _context.TaiKhoans
                    .FirstOrDefaultAsync(x =>
                        x.MaTaiKhoan == maTaiKhoan);

            if (taiKhoan == null)
            {
                TempData["Error"] =
                    "Không tìm thấy tài khoản.";

                return RedirectToAction(nameof(Index));
            }

            taiKhoan.TrangThai = false;

            await _context.SaveChangesAsync();

            TempData["Success"] =
                "Đã khóa tài khoản thành công.";

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MoKhoaTaiKhoan(
            int maTaiKhoan)
        {
            var taiKhoan =
                await _context.TaiKhoans
                    .FirstOrDefaultAsync(x =>
                        x.MaTaiKhoan == maTaiKhoan);

            if (taiKhoan == null)
            {
                TempData["Error"] =
                    "Không tìm thấy tài khoản.";

                return RedirectToAction(nameof(Index));
            }

            /*
             * Người chăm sóc chưa được duyệt
             * không được mở khóa bằng nút thông thường.
             */
            var nguoiChamSoc =
                await _context.NguoiChamSocs
                    .FirstOrDefaultAsync(x =>
                        x.MaTaiKhoan == maTaiKhoan);

            if (nguoiChamSoc != null &&
                nguoiChamSoc.TrangThai != "Đã duyệt")
            {
                TempData["Error"] =
                    "Hồ sơ người chăm sóc chưa được phê duyệt.";

                return RedirectToAction(nameof(Index));
            }

            taiKhoan.TrangThai = true;

            await _context.SaveChangesAsync();

            TempData["Success"] =
                "Đã mở khóa tài khoản thành công.";

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DuyetNguoiChamSoc(
            int maNguoiChamSoc)
        {
            var nguoiChamSoc =
                await _context.NguoiChamSocs
                    .FirstOrDefaultAsync(x =>
                        x.MaNguoiChamSoc ==
                        maNguoiChamSoc);

            if (nguoiChamSoc == null)
            {
                TempData["Error"] =
                    "Không tìm thấy hồ sơ người chăm sóc.";

                return RedirectToAction(nameof(Index));
            }

            nguoiChamSoc.TrangThai =
                "Đã duyệt";

            if (nguoiChamSoc.MaTaiKhoan.HasValue)
            {
                var taiKhoan =
                    await _context.TaiKhoans
                        .FirstOrDefaultAsync(x =>
                            x.MaTaiKhoan ==
                            nguoiChamSoc.MaTaiKhoan.Value);

                if (taiKhoan != null)
                {
                    taiKhoan.TrangThai = true;
                }
            }

            await _context.SaveChangesAsync();

            TempData["Success"] =
                $"Đã duyệt hồ sơ của {nguoiChamSoc.HoTen}.";

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> TuChoiNguoiChamSoc(
            int maNguoiChamSoc)
        {
            var nguoiChamSoc =
                await _context.NguoiChamSocs
                    .FirstOrDefaultAsync(x =>
                        x.MaNguoiChamSoc ==
                        maNguoiChamSoc);

            if (nguoiChamSoc == null)
            {
                TempData["Error"] =
                    "Không tìm thấy hồ sơ người chăm sóc.";

                return RedirectToAction(nameof(Index));
            }

            nguoiChamSoc.TrangThai =
                "Từ chối";

            if (nguoiChamSoc.MaTaiKhoan.HasValue)
            {
                var taiKhoan =
                    await _context.TaiKhoans
                        .FirstOrDefaultAsync(x =>
                            x.MaTaiKhoan ==
                            nguoiChamSoc.MaTaiKhoan.Value);

                if (taiKhoan != null)
                {
                    taiKhoan.TrangThai = false;
                }
            }

            await _context.SaveChangesAsync();

            TempData["Success"] =
                $"Đã từ chối hồ sơ của {nguoiChamSoc.HoTen}.";

            return RedirectToAction(nameof(Index));
        }
    }
}