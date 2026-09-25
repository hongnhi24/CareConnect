using CareConnect.Data;
using CareConnect.Models;
using CareConnect.Services;
using CareConnect.ViewModels;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Text.Json;

namespace CareConnect.Controllers
{
    public class AccountController : Controller
    {
        private readonly CareConnectDbContext _context;
        private readonly IPasswordSecurityService
            _passwordSecurity;

        public AccountController(
            CareConnectDbContext context,
            IPasswordSecurityService passwordSecurity)
        {
            _context = context;
            _passwordSecurity = passwordSecurity;
        }
        [AllowAnonymous]
        [HttpGet]
        public IActionResult CompleteCaregiverRegistration()
        {
            string? duLieuBuoc1 = HttpContext.Session.GetString(
                "RegisterCaregiverStep1");

            string? duLieuBuoc2 = HttpContext.Session.GetString(
                "RegisterCaregiverStep2");

            string? duLieuBuoc3 = HttpContext.Session.GetString(
                "RegisterCaregiverStep3");

            if (string.IsNullOrWhiteSpace(duLieuBuoc1))
            {
                return RedirectToAction(nameof(RegisterCaregiver));
            }

            if (string.IsNullOrWhiteSpace(duLieuBuoc2))
            {
                return RedirectToAction(nameof(RegisterCaregiverStep2));
            }

            if (string.IsNullOrWhiteSpace(duLieuBuoc3))
            {
                return RedirectToAction(nameof(RegisterCaregiverStep3));
            }

            return View();
        }
        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> Login(
    string? returnUrl = null)
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                return await RedirectTheoVaiTroAsync(User);
            }

            return View(new LoginViewModel
            {
                ReturnUrl = returnUrl
            });
        }
        [AllowAnonymous]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(
            LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            string tenDangNhap = model.TenDangNhap.Trim();

            string thongTinDangNhap = model.TenDangNhap.Trim();

            TaiKhoan? taiKhoan = await _context.TaiKhoans
                .AsNoTracking()
                .Include(t => t.VaiTro)
                .FirstOrDefaultAsync(t =>
                    t.TenDangNhap == thongTinDangNhap ||
                    t.Email == thongTinDangNhap);

            if (taiKhoan == null)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "Email, số điện thoại hoặc mật khẩu không đúng.");

                return View(model);
            }

            if (!taiKhoan.TrangThai)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "Tài khoản đã bị khóa hoặc chưa được kích hoạt.");

                return View(model);
            }

            bool matKhauHopLe =
                await _passwordSecurity
                    .VerifyPasswordAsync(
                        taiKhoan,
                        model.MatKhau);

            if (!matKhauHopLe)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "Email, tên đăng nhập hoặc mật khẩu không đúng.");

                return View(model);
            }

            string tenVaiTro =
                taiKhoan.VaiTro?.TenVaiTro ?? "Không xác định";

            var claims = new List<Claim>
            {
                new(
                    ClaimTypes.NameIdentifier,
                    taiKhoan.MaTaiKhoan.ToString()),

                  new(
        ClaimTypes.Name,
        taiKhoan.TenDangNhap),

                new(
                    ClaimTypes.Email,
                    taiKhoan.Email ?? string.Empty),

                new(
                    ClaimTypes.Role,
                    tenVaiTro)
            };

            var claimsIdentity = new ClaimsIdentity(
                claims,
                CookieAuthenticationDefaults.AuthenticationScheme);

            var authProperties = new AuthenticationProperties
            {
                IsPersistent = model.GhiNhoDangNhap,

                ExpiresUtc = model.GhiNhoDangNhap
                    ? DateTimeOffset.UtcNow.AddDays(7)
                    : DateTimeOffset.UtcNow.AddHours(8),

                AllowRefresh = true
            };

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity),
                authProperties);

            if (!string.IsNullOrWhiteSpace(model.ReturnUrl) &&
                Url.IsLocalUrl(model.ReturnUrl))
            {
                return LocalRedirect(model.ReturnUrl);
            }

            return await RedirectTheoVaiTroAsync(
    new ClaimsPrincipal(claimsIdentity));
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(
                CookieAuthenticationDefaults.AuthenticationScheme);

            return RedirectToAction("Index", "Home");
        }
        [AllowAnonymous]
        [HttpGet]
        public IActionResult Register()
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                return RedirectToAction("Index", "Home");
            }

            return View();
        }

        [AllowAnonymous]
        [HttpGet]
        public IActionResult RegisterCustomer()
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                return RedirectToAction("Index", "Home");
            }

            return View(new RegisterCustomerViewModel());
        }
        [AllowAnonymous]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RegisterCustomer(
    RegisterCustomerViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            string email = model.Email.Trim().ToLowerInvariant();
            string soDienThoai = model.SoDienThoai.Trim();
            string hoTen = model.HoTen.Trim();

            bool emailDaTonTai = await _context.TaiKhoans
                .AnyAsync(t => t.Email == email);

            if (emailDaTonTai)
            {
                ModelState.AddModelError(
                    nameof(model.Email),
                    "Email này đã được sử dụng.");

                return View(model);
            }

            bool soDienThoaiDaTonTai = await _context.KhachHangs
                .AnyAsync(k => k.SoDienThoai == soDienThoai);

            if (soDienThoaiDaTonTai)
            {
                ModelState.AddModelError(
                    nameof(model.SoDienThoai),
                    "Số điện thoại này đã được sử dụng.");

                return View(model);
            }

            VaiTro? vaiTroKhachHang = await _context.VaiTros
                .FirstOrDefaultAsync(v =>
                    v.TenVaiTro == "Khách hàng");

            if (vaiTroKhachHang == null)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "Không tìm thấy vai trò Khách hàng trong hệ thống.");

                return View(model);
            }

            await using var transaction =
                await _context.Database.BeginTransactionAsync();

            try
            {
                var taiKhoan = new TaiKhoan
                {
                    TenDangNhap = email,
                    Email = email,

                    MatKhau = string.Empty,

                    MaVaiTro = vaiTroKhachHang.MaVaiTro,
                    TrangThai = true,
                    NgayTao = DateTime.Now
                };

                taiKhoan.MatKhau =
                    _passwordSecurity.HashPassword(
                        taiKhoan,
                        model.MatKhau);

                _context.TaiKhoans.Add(taiKhoan);
                await _context.SaveChangesAsync();

                var khachHang = new KhachHang
                {
                    MaTaiKhoan = taiKhoan.MaTaiKhoan,
                    HoTen = hoTen,
                    SoDienThoai = soDienThoai,
                    Email = email,
                    NgayTao = DateTime.Now
                };

                _context.KhachHangs.Add(khachHang);
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();

                TempData["RegisterSuccess"] =
                    "Đăng ký thành công. Bạn có thể đăng nhập ngay.";

                return RedirectToAction(nameof(Login));
            }
            catch
            {
                await transaction.RollbackAsync();

                ModelState.AddModelError(
                    string.Empty,
                    "Không thể tạo tài khoản. Vui lòng thử lại.");

                return View(model);
            }
        }

        [AllowAnonymous]
        [HttpGet]
        public IActionResult RegisterCaregiver()
        {
            return View();
        }

        [AllowAnonymous]
        public IActionResult AccessDenied()
        {
            return View();
        }



        [AllowAnonymous]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RegisterCaregiver(
    RegisterCaregiverViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            string email = model.Email
                .Trim()
                .ToLowerInvariant();

            string soDienThoai = model.SoDienThoai.Trim();

            bool emailDaTonTai = await _context.TaiKhoans
                .AnyAsync(t => t.Email == email);

            if (emailDaTonTai)
            {
                ModelState.AddModelError(
                    nameof(model.Email),
                    "Email này đã được sử dụng.");

                return View(model);
            }

            bool soDienThoaiDaTonTai =
                await _context.NguoiChamSocs
                    .AnyAsync(ncs =>
                        ncs.SoDienThoai == soDienThoai);

            if (soDienThoaiDaTonTai)
            {
                ModelState.AddModelError(
                    nameof(model.SoDienThoai),
                    "Số điện thoại này đã được sử dụng.");

                return View(model);
            }

            var taiKhoanTam =
                new TaiKhoan
                {
                    TenDangNhap = email,
                    Email = email
                };

            string matKhauDaBam =
                _passwordSecurity.HashPassword(
                    taiKhoanTam,
                    model.MatKhau);

            /*
             * Session chỉ giữ hash, không giữ
             * mật khẩu dạng thường trong quá trình
             * đăng ký nhiều bước của caregiver.
             */
            var duLieuBuoc1 =
                new RegisterCaregiverViewModel
                {
                    HoTen = model.HoTen.Trim(),
                    SoDienThoai = soDienThoai,
                    Email = email,
                    MatKhau = matKhauDaBam,
                    XacNhanMatKhau = matKhauDaBam
                };

            string jsonBuoc1 =
                JsonSerializer.Serialize(duLieuBuoc1);

            HttpContext.Session.SetString(
                "RegisterCaregiverStep1",
                jsonBuoc1);

            return RedirectToAction(
                nameof(RegisterCaregiverStep2));
        }


        [AllowAnonymous]
        [HttpGet]
        public IActionResult RegisterCaregiverStep2()
        {
            string? duLieuBuoc1 = HttpContext.Session.GetString(
                "RegisterCaregiverStep1");

            if (string.IsNullOrWhiteSpace(duLieuBuoc1))
            {
                return RedirectToAction(nameof(RegisterCaregiver));
            }

            string? duLieuBuoc2 = HttpContext.Session.GetString(
                "RegisterCaregiverStep2");

            if (!string.IsNullOrWhiteSpace(duLieuBuoc2))
            {
                RegisterCaregiverStep2ViewModel? modelCu =
                    JsonSerializer.Deserialize<RegisterCaregiverStep2ViewModel>(
                        duLieuBuoc2);

                if (modelCu != null)
                {
                    return View(modelCu);
                }
            }

            return View(new RegisterCaregiverStep2ViewModel());
        }

        [AllowAnonymous]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult RegisterCaregiverStep2(
            RegisterCaregiverStep2ViewModel model)
        {
            string? duLieuBuoc1 = HttpContext.Session.GetString(
                "RegisterCaregiverStep1");

            if (string.IsNullOrWhiteSpace(duLieuBuoc1))
            {
                return RedirectToAction(nameof(RegisterCaregiver));
            }

            if (model.ChuyenMon == null ||
                model.ChuyenMon.Count == 0)
            {
                ModelState.AddModelError(
                    nameof(model.ChuyenMon),
                    "Vui lòng chọn ít nhất một chuyên môn.");
            }

            if (model.NgayCap.HasValue &&
                model.NgayCap.Value.Date > DateTime.Today)
            {
                ModelState.AddModelError(
                    nameof(model.NgayCap),
                    "Ngày cấp không được lớn hơn ngày hiện tại.");
            }

            if (model.NgayCap.HasValue &&
                model.NgayHetHan.HasValue &&
                model.NgayHetHan.Value.Date <= model.NgayCap.Value.Date)
            {
                ModelState.AddModelError(
                    nameof(model.NgayHetHan),
                    "Ngày hết hạn phải lớn hơn ngày cấp.");
            }

            bool daNhapThongTinChungChi =
                !string.IsNullOrWhiteSpace(model.TenChungChi) ||
                !string.IsNullOrWhiteSpace(model.DonViCap) ||
                model.NgayCap.HasValue ||
                model.NgayHetHan.HasValue;

            if (daNhapThongTinChungChi)
            {
                if (string.IsNullOrWhiteSpace(model.TenChungChi))
                {
                    ModelState.AddModelError(
                        nameof(model.TenChungChi),
                        "Vui lòng nhập tên chứng chỉ.");
                }

                if (string.IsNullOrWhiteSpace(model.DonViCap))
                {
                    ModelState.AddModelError(
                        nameof(model.DonViCap),
                        "Vui lòng nhập đơn vị cấp.");
                }

                if (!model.NgayCap.HasValue)
                {
                    ModelState.AddModelError(
                        nameof(model.NgayCap),
                        "Vui lòng chọn ngày cấp.");
                }
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            model.ChuyenMon = model.ChuyenMon
                .Where(c => !string.IsNullOrWhiteSpace(c))
                .Distinct()
                .ToList();

            string duLieuBuoc2 = JsonSerializer.Serialize(model);

            HttpContext.Session.SetString(
                "RegisterCaregiverStep2",
                duLieuBuoc2);

            return RedirectToAction(nameof(RegisterCaregiverStep3));
        }

        [AllowAnonymous]
        [HttpGet]
        public IActionResult RegisterCaregiverStep3()
        {
            string? duLieuBuoc1 = HttpContext.Session.GetString(
                "RegisterCaregiverStep1");

            string? duLieuBuoc2 = HttpContext.Session.GetString(
                "RegisterCaregiverStep2");

            if (string.IsNullOrWhiteSpace(duLieuBuoc1))
            {
                return RedirectToAction(nameof(RegisterCaregiver));
            }

            if (string.IsNullOrWhiteSpace(duLieuBuoc2))
            {
                return RedirectToAction(nameof(RegisterCaregiverStep2));
            }

            string? duLieuBuoc3 = HttpContext.Session.GetString(
                "RegisterCaregiverStep3");

            if (!string.IsNullOrWhiteSpace(duLieuBuoc3))
            {
                RegisterCaregiverStep3ViewModel? modelCu =
                    JsonSerializer.Deserialize<RegisterCaregiverStep3ViewModel>(
                        duLieuBuoc3);

                if (modelCu != null)
                {
                    return View(modelCu);
                }
            }

            return View(new RegisterCaregiverStep3ViewModel());
        }
        [HttpGet]
        public async Task<IActionResult> LogoutAfterDelete()
        {
            await HttpContext.SignOutAsync(
                CookieAuthenticationDefaults
                    .AuthenticationScheme);

            TempData["Success"] =
                "Tài khoản của bạn đã được xóa.";

            return RedirectToAction(
                "Login",
                "Account");
        }
        [AllowAnonymous]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RegisterCaregiverStep3(
    RegisterCaregiverStep3ViewModel model)
        {
            string? duLieuBuoc1 =
                HttpContext.Session.GetString("RegisterCaregiverStep1");

            string? duLieuBuoc2 =
                HttpContext.Session.GetString("RegisterCaregiverStep2");

            if (string.IsNullOrWhiteSpace(duLieuBuoc1))
            {
                return RedirectToAction(nameof(RegisterCaregiver));
            }

            if (string.IsNullOrWhiteSpace(duLieuBuoc2))
            {
                return RedirectToAction(nameof(RegisterCaregiverStep2));
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            RegisterCaregiverViewModel? buoc1 =
                JsonSerializer.Deserialize<RegisterCaregiverViewModel>(
                    duLieuBuoc1);

            RegisterCaregiverStep2ViewModel? buoc2 =
                JsonSerializer.Deserialize<RegisterCaregiverStep2ViewModel>(
                    duLieuBuoc2);

            if (buoc1 == null || buoc2 == null)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "Dữ liệu đăng ký không hợp lệ. Vui lòng đăng ký lại.");

                return View(model);
            }

            string email =
                buoc1.Email.Trim().ToLowerInvariant();

            bool emailDaTonTai =
                await _context.TaiKhoans.AnyAsync(x =>
                    x.Email == email);

            if (emailDaTonTai)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "Email này đã được sử dụng.");

                return View(model);
            }

            VaiTro? vaiTroNguoiChamSoc =
                await _context.VaiTros.FirstOrDefaultAsync(x =>
                    x.TenVaiTro == "Người chăm sóc");

            if (vaiTroNguoiChamSoc == null)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "Không tìm thấy vai trò Người chăm sóc.");

                return View(model);
            }

            await using var transaction =
                await _context.Database.BeginTransactionAsync();

            try
            {
                var taiKhoan = new TaiKhoan
                {
                    TenDangNhap = email,
                    Email = email,

                    // buoc1.MatKhau đã là hash
                    // từ bước đăng ký đầu tiên.
                    MatKhau = buoc1.MatKhau,

                    MaVaiTro =
                        vaiTroNguoiChamSoc.MaVaiTro,

                    // Chưa được phép đăng nhập trước khi admin duyệt
                    TrangThai = false,

                    NgayTao = DateTime.Now
                };

                _context.TaiKhoans.Add(taiKhoan);

                await _context.SaveChangesAsync();

                var nguoiChamSoc = new NguoiChamSoc
                {
                    MaTaiKhoan = taiKhoan.MaTaiKhoan,

                    HoTen = buoc1.HoTen.Trim(),

                    SoDienThoai =
                        buoc1.SoDienThoai.Trim(),

                    Email = email,

                    BangCap =
                        buoc2.BangCap.Trim(),

                    KinhNghiem =
                        buoc2.SoNamKinhNghiem ?? 0,

                    ChuyenMon =
                        string.Join(", ", buoc2.ChuyenMon),

                    GiaTheoGio =
                        model.GiaMoiGio,

                    DanhGia = 0,

                    TrangThai = "Chờ duyệt",

                    NgayTao = DateTime.Now,

                    KhuVucHoatDong =
                        model.KhuVucHoatDong.Trim(),

                    GioiThieu =
                        model.GioiThieu.Trim()
                };

                _context.NguoiChamSocs.Add(nguoiChamSoc);

                await _context.SaveChangesAsync();

                await transaction.CommitAsync();

                HttpContext.Session.Remove(
                    "RegisterCaregiverStep1");

                HttpContext.Session.Remove(
                    "RegisterCaregiverStep2");

                HttpContext.Session.Remove(
                    "RegisterCaregiverStep3");

                return RedirectToAction(
                    nameof(CompleteCaregiverRegistration));
            }
            catch
            {
                await transaction.RollbackAsync();

                ModelState.AddModelError(
                    string.Empty,
                    "Không thể gửi hồ sơ. Vui lòng thử lại.");

                return View(model);
            }
        }
        private async Task<IActionResult> RedirectTheoVaiTroAsync(
    ClaimsPrincipal user)
        {
            // ADMIN
            if (user.IsInRole("Quản trị viên"))
            {
                return RedirectToAction(
                    "Index",
                    "Dashboard",
                    new { area = "Admin" });
            }

            // KHÁCH HÀNG
            if (user.IsInRole("Khách hàng"))
            {
                return RedirectToAction(
                    "Index",
                    "Dashboard",
                    new { area = "Customer" });
            }

            // NGƯỜI CHĂM SÓC
            if (user.IsInRole("Người chăm sóc"))
            {
                string? maTaiKhoanClaim =
                    user.FindFirstValue(
                        ClaimTypes.NameIdentifier);

                if (!int.TryParse(
                        maTaiKhoanClaim,
                        out int maTaiKhoan))
                {
                    await HttpContext.SignOutAsync(
                        CookieAuthenticationDefaults
                            .AuthenticationScheme);

                    return RedirectToAction(
                        nameof(Login));
                }

                var nguoiChamSoc =
                    await _context.NguoiChamSocs
                        .AsNoTracking()
                        .FirstOrDefaultAsync(x =>
                            x.MaTaiKhoan == maTaiKhoan);

                if (nguoiChamSoc == null)
                {
                    await HttpContext.SignOutAsync(
                        CookieAuthenticationDefaults
                            .AuthenticationScheme);

                    TempData["LoginError"] =
                        "Tài khoản chưa được liên kết "
                        + "với hồ sơ người chăm sóc.";

                    return RedirectToAction(
                        nameof(Login));
                }

                string trangThaiHoSo =
                    nguoiChamSoc.TrangThai?.Trim()
                    ?? "Chưa hoàn thiện";

                switch (trangThaiHoSo)
                {
                    case "Chưa hoàn thiện":
                        return RedirectToAction(
                            "Index",
                            "ProfileSetup",
                            new { area = "Caregiver" });

                    case "Chờ duyệt":
                        return RedirectToAction(
                            "Pending",
                            "ProfileSetup",
                            new { area = "Caregiver" });

                    case "Đã duyệt":
                        return RedirectToAction(
                            "Index",
                            "Dashboard",
                            new { area = "Caregiver" });

                    case "Từ chối":
                        TempData["ProfileMessage"] =
                            "Hồ sơ của bạn chưa được chấp thuận. "
                            + "Vui lòng cập nhật lại thông tin.";

                        return RedirectToAction(
                            "Index",
                            "ProfileSetup",
                            new { area = "Caregiver" });

                    default:
                        TempData["ProfileMessage"] =
                            $"Trạng thái hồ sơ hiện tại: "
                            + $"{trangThaiHoSo}. "
                            + "Vui lòng hoàn thiện hồ sơ.";

                        return RedirectToAction(
                            "Index",
                            "ProfileSetup",
                            new { area = "Caregiver" });
                }
            }
            return RedirectToAction(
                "Index",
                "Home");
        }
    }
}
