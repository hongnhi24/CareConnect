using System.Security.Claims;
using CareConnect.Data;
using CareConnect.Models;
using CareConnect.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CareConnect.Areas.Caregiver.Controllers
{
    [Area("Caregiver")]
    [Authorize(Roles = "Người chăm sóc")]
    public class CertificatesController : Controller
    {
        private readonly CareConnectDbContext _context;


        public CertificatesController(
            CareConnectDbContext context)
        {
            _context = context;
        }


        // ============================================
        // THƯ MỤC LƯU FILE CHỨNG CHỈ
        // ============================================
        private string LayThuMucChungChi()
        {
            string folder =
                Path.Combine(
                    Environment.GetFolderPath(
                        Environment.SpecialFolder
                            .LocalApplicationData),
                    "CareConnect",
                    "Certificates");


            Directory.CreateDirectory(folder);


            return folder;
        }


        // ============================================
        // LẤY MÃ TÀI KHOẢN ĐANG ĐĂNG NHẬP
        // ============================================
        private int? LayMaTaiKhoan()
        {
            string? claim =
                User.FindFirstValue(
                    ClaimTypes.NameIdentifier);


            if (!int.TryParse(
                claim,
                out int maTaiKhoan))
            {
                return null;
            }


            return maTaiKhoan;
        }


        // ============================================
        // LẤY MÃ NGƯỜI CHĂM SÓC
        // ============================================
        private async Task<int?> LayMaNguoiChamSoc()
        {
            int? maTaiKhoan =
                LayMaTaiKhoan();


            if (!maTaiKhoan.HasValue)
            {
                return null;
            }


            return await _context.NguoiChamSocs
                .AsNoTracking()
                .Where(x =>
                    x.MaTaiKhoan
                        == maTaiKhoan.Value)
                .Select(x =>
                    (int?)x.MaNguoiChamSoc)
                .FirstOrDefaultAsync();
        }


        // ============================================
        // DANH SÁCH CHỨNG CHỈ
        // ============================================
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            int? maNguoiChamSoc =
                await LayMaNguoiChamSoc();


            if (!maNguoiChamSoc.HasValue)
            {
                return Forbid();
            }


            var danhSach =
                await _context.ChungChis
                    .AsNoTracking()
                    .Where(x =>
                        x.MaNguoiChamSoc
                            == maNguoiChamSoc.Value)
                    .OrderByDescending(x =>
                        x.MaChungChi)
                    .ToListAsync();


            return View(danhSach);
        }


        // ============================================
        // MỞ FORM THÊM CHỨNG CHỈ
        // ============================================
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            int? maNguoiChamSoc =
                await LayMaNguoiChamSoc();


            if (!maNguoiChamSoc.HasValue)
            {
                return Forbid();
            }


            return View(
                new CaregiverCertificateViewModel());
        }

        // ============================================
        // GỬI / LƯU CHỨNG CHỈ
        // ============================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            CaregiverCertificateViewModel model)
        {
            int? maNguoiChamSoc =
                await LayMaNguoiChamSoc();

            if (!maNguoiChamSoc.HasValue)
            {
                return Forbid();
            }


            // ========================================
            // KIỂM TRA TÊN CHỨNG CHỈ
            // ========================================

            if (string.IsNullOrWhiteSpace(
                model.TenChungChi))
            {
                ModelState.AddModelError(
                    nameof(model.TenChungChi),
                    "Vui lòng nhập tên chứng chỉ.");
            }


            // ========================================
            // KIỂM TRA NGÀY
            // ========================================

            if (
                model.NgayCap.HasValue
                &&
                model.NgayHetHan.HasValue
                &&
                model.NgayHetHan.Value.Date
                    < model.NgayCap.Value.Date)
            {
                ModelState.AddModelError(
                    nameof(model.NgayHetHan),
                    "Ngày hết hạn phải sau ngày cấp.");
            }


            // ========================================
            // KIỂM TRA FILE
            // ========================================

            if (
                model.FileChungChi == null
                ||
                model.FileChungChi.Length == 0)
            {
                ModelState.AddModelError(
                    nameof(model.FileChungChi),
                    "Vui lòng chọn file chứng chỉ.");
            }
            else
            {
                string extension =
                    Path.GetExtension(
                        model.FileChungChi.FileName)
                    .ToLowerInvariant();


                string[] allowedExtensions =
                {
            ".jpg",
            ".jpeg",
            ".png",
            ".pdf"
        };


                if (!allowedExtensions.Contains(
                    extension))
                {
                    ModelState.AddModelError(
                        nameof(model.FileChungChi),
                        "Chỉ hỗ trợ JPG, JPEG, PNG hoặc PDF.");
                }


                const long maxFileSize =
                    5 * 1024 * 1024;


                if (
                    model.FileChungChi.Length
                        > maxFileSize)
                {
                    ModelState.AddModelError(
                        nameof(model.FileChungChi),
                        "Dung lượng file tối đa là 5MB.");
                }
            }


            if (!ModelState.IsValid)
            {
                return View(model);
            }


            string? fullPath = null;


            try
            {
                // ========================================
                // THƯ MỤC LƯU FILE
                // ========================================

                string folder =
                    LayThuMucChungChi();


                // ========================================
                // TẠO TÊN FILE MỚI
                // ========================================

                string extension =
                    Path.GetExtension(
                        model.FileChungChi!.FileName)
                    .ToLowerInvariant();


                string fileName =
                    $"{Guid.NewGuid():N}{extension}";


                fullPath =
                    Path.Combine(
                        folder,
                        fileName);


                // ========================================
                // LƯU FILE THẬT
                // ========================================

                await using (
                    FileStream stream =
                        new FileStream(
                            fullPath,
                            FileMode.Create,
                            FileAccess.Write))
                {
                    await model.FileChungChi
                        .CopyToAsync(stream);
                }


                // ========================================
                // KIỂM TRA FILE ĐÃ LƯU
                // ========================================

                if (!System.IO.File.Exists(
                    fullPath))
                {
                    throw new IOException(
                        "Không thể lưu file chứng chỉ.");
                }


                // ========================================
                // TẠO RECORD CHỨNG CHỈ
                // ========================================

                var chungChi =
                    new ChungChi
                    {
                        MaNguoiChamSoc =
                            maNguoiChamSoc.Value,

                        TenChungChi =
                            model.TenChungChi.Trim(),

                        DonViCap =
                            string.IsNullOrWhiteSpace(
                                model.DonViCap)
                                ? null
                                : model.DonViCap.Trim(),

                        NgayCap =
                            model.NgayCap?.Date,

                        NgayHetHan =
                            model.NgayHetHan?.Date,

                        FileChungChi =
                            fileName,

                        TrangThaiXacMinh =
                            "Chờ xác minh",

                        NgayXacMinh =
                            null,

                        GhiChuXacMinh =
                            null,

                        PhuongThucXacMinh =
                            null
                    };


                // ========================================
                // LƯU DATABASE
                // ========================================

                _context.ChungChis.Add(
                    chungChi);


                await _context.SaveChangesAsync();


                TempData["Success"] =
                    "Đã gửi chứng chỉ thành công. "
                    + "Vui lòng chờ quản trị viên xác minh.";


                return RedirectToAction(
                    nameof(Index));
            }
            catch (Exception ex)
            {
                // Nếu DB lỗi thì xóa file vừa lưu
                if (
                    !string.IsNullOrWhiteSpace(
                        fullPath)
                    &&
                    System.IO.File.Exists(
                        fullPath))
                {
                    try
                    {
                        System.IO.File.Delete(
                            fullPath);
                    }
                    catch
                    {
                    }
                }


                ModelState.AddModelError(
                    string.Empty,
                    "Gửi chứng chỉ thất bại: "
                    + ex.Message);


                return View(model);
            }
        }
        // ============================================
        // MỞ TRANG BỔ SUNG FILE CHỨNG CHỈ
        // ============================================
        [HttpGet]
        public async Task<IActionResult> EditFile(
            int id)
        {
            int? maNguoiChamSoc =
                await LayMaNguoiChamSoc();


            if (!maNguoiChamSoc.HasValue)
            {
                return Forbid();
            }


            var chungChi =
                await _context.ChungChis
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x =>
                        x.MaChungChi == id
                        &&
                        x.MaNguoiChamSoc
                            == maNguoiChamSoc.Value);


            if (chungChi == null)
            {
                return NotFound();
            }


            // Chỉ cho bổ sung nếu chưa có file
            if (!string.IsNullOrWhiteSpace(
                chungChi.FileChungChi))
            {
                TempData["Error"] =
                    "Chứng chỉ này đã có file đính kèm.";

                return RedirectToAction(
                    nameof(Index));
            }


            var model =
                new CaregiverCertificateFileViewModel
                {
                    MaChungChi =
                        chungChi.MaChungChi,

                    TenChungChi =
                        chungChi.TenChungChi
                        ?? string.Empty,

                    DonViCap =
                        chungChi.DonViCap
                };


            return View(model);
        }
        // ============================================
        // LƯU FILE BỔ SUNG
        // ============================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditFile(
            CaregiverCertificateFileViewModel model)
        {
            int? maNguoiChamSoc =
                await LayMaNguoiChamSoc();


            if (!maNguoiChamSoc.HasValue)
            {
                return Forbid();
            }


            var chungChi =
                await _context.ChungChis
                    .FirstOrDefaultAsync(x =>
                        x.MaChungChi
                            == model.MaChungChi
                        &&
                        x.MaNguoiChamSoc
                            == maNguoiChamSoc.Value);


            if (chungChi == null)
            {
                return NotFound();
            }


            // Không cho ghi đè file đã tồn tại
            if (!string.IsNullOrWhiteSpace(
                chungChi.FileChungChi))
            {
                TempData["Error"] =
                    "Chứng chỉ này đã có file đính kèm.";

                return RedirectToAction(
                    nameof(Index));
            }


            // Để hiển thị lại nếu validation lỗi
            model.TenChungChi =
                chungChi.TenChungChi
                ?? string.Empty;

            model.DonViCap =
                chungChi.DonViCap;


            // ========================================
            // KIỂM TRA FILE
            // ========================================

            if (model.FileChungChi == null ||
                model.FileChungChi.Length == 0)
            {
                ModelState.AddModelError(
                    nameof(model.FileChungChi),
                    "Vui lòng chọn file chứng chỉ.");
            }
            else
            {
                string extension =
                    Path.GetExtension(
                        model.FileChungChi.FileName)
                    .ToLowerInvariant();


                string[] allowedExtensions =
                {
            ".jpg",
            ".jpeg",
            ".png",
            ".pdf"
        };


                if (!allowedExtensions.Contains(
                    extension))
                {
                    ModelState.AddModelError(
                        nameof(model.FileChungChi),
                        "Chỉ hỗ trợ JPG, JPEG, PNG hoặc PDF.");
                }


                const long maxFileSize =
                    5 * 1024 * 1024;


                if (model.FileChungChi.Length >
                    maxFileSize)
                {
                    ModelState.AddModelError(
                        nameof(model.FileChungChi),
                        "Dung lượng file tối đa là 5MB.");
                }
            }


            if (!ModelState.IsValid)
            {
                return View(model);
            }


            string? fullPath = null;


            try
            {
                // ========================================
                // LƯU FILE
                // ========================================

                string folder =
                    LayThuMucChungChi();


                string extension =
                    Path.GetExtension(
                        model.FileChungChi!.FileName)
                    .ToLowerInvariant();


                string fileName =
                    $"{Guid.NewGuid():N}{extension}";


                fullPath =
                    Path.Combine(
                        folder,
                        fileName);


                await using (
                    FileStream stream =
                        new FileStream(
                            fullPath,
                            FileMode.Create,
                            FileAccess.Write))
                {
                    await model.FileChungChi
                        .CopyToAsync(stream);
                }


                // ========================================
                // UPDATE DATABASE
                // ========================================

                chungChi.FileChungChi =
                    fileName;


                // Có file thật rồi thì phải xác minh lại
                chungChi.TrangThaiXacMinh =
                    "Chờ xác minh";


                chungChi.NgayXacMinh =
                    null;


                chungChi.GhiChuXacMinh =
                    null;


                chungChi.PhuongThucXacMinh =
                    null;


                await _context.SaveChangesAsync();


                TempData["Success"] =
                    "Đã bổ sung file chứng chỉ thành công. "
                    + "Vui lòng chờ quản trị viên xác minh.";


                return RedirectToAction(
                    nameof(Index));
            }
            catch (Exception ex)
            {
                // Nếu UPDATE DB lỗi thì xóa file mới
                if (!string.IsNullOrWhiteSpace(
                        fullPath)
                    &&
                    System.IO.File.Exists(
                        fullPath))
                {
                    try
                    {
                        System.IO.File.Delete(
                            fullPath);
                    }
                    catch
                    {
                    }
                }


                ModelState.AddModelError(
                    string.Empty,
                    "Không thể bổ sung file: "
                    + ex.Message);


                return View(model);
            }
        }
        // ============================================
        // XEM FILE CHỨNG CHỈ
        // ============================================
        [HttpGet]
        public async Task<IActionResult> ViewFile(
            int id)
        {
            int? maNguoiChamSoc =
                await LayMaNguoiChamSoc();


            if (!maNguoiChamSoc.HasValue)
            {
                return Forbid();
            }


            // ========================================
            // CHỈ CHO XEM CHỨNG CHỈ CỦA CHÍNH MÌNH
            // ========================================
            var chungChi =
                await _context.ChungChis
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x =>
                        x.MaChungChi == id
                        &&
                        x.MaNguoiChamSoc
                            == maNguoiChamSoc.Value);


            if (chungChi == null)
            {
                return NotFound();
            }


            // ========================================
            // CHƯA CÓ FILE
            // ========================================
            if (string.IsNullOrWhiteSpace(
                chungChi.FileChungChi))
            {
                TempData["Error"] =
                    "Chứng chỉ này chưa có file đính kèm.";


                return RedirectToAction(
                    nameof(Index));
            }


            // ========================================
            // CHẶN TÊN FILE KHÔNG HỢP LỆ
            // ========================================
            string fileName =
                Path.GetFileName(
                    chungChi.FileChungChi);


            if (fileName !=
                chungChi.FileChungChi)
            {
                return NotFound();
            }


            // ========================================
            // ĐƯỜNG DẪN FILE
            // ========================================
            string filePath =
                Path.Combine(
                    LayThuMucChungChi(),
                    fileName);


            if (!System.IO.File.Exists(
                filePath))
            {
                TempData["Error"] =
                    "Không tìm thấy file chứng chỉ "
                    + "trên hệ thống.";


                return RedirectToAction(
                    nameof(Index));
            }


            // ========================================
            // XÁC ĐỊNH LOẠI FILE
            // ========================================
            string extension =
                Path.GetExtension(
                    filePath)
                .ToLowerInvariant();


            string contentType =
                extension switch
                {
                    ".jpg" =>
                        "image/jpeg",

                    ".jpeg" =>
                        "image/jpeg",

                    ".png" =>
                        "image/png",

                    ".pdf" =>
                        "application/pdf",

                    _ =>
                        "application/octet-stream"
                };


            // ========================================
            // TRẢ FILE CHO TRÌNH DUYỆT
            // ========================================
            return PhysicalFile(
                filePath,
                contentType);
        }
    }
}