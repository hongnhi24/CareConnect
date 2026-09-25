using CareConnect.Data;
using CareConnect.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CareConnect.Areas.Admin.Controllers
{
    [Area("Admin")]

    // Nếu các controller Admin hiện tại của bạn
    // dùng role khác, copy đúng Role từ controller Admin đó.
    [Authorize(Roles = "Quản trị viên")]

    public class CertificatesController : Controller
    {
        private readonly CareConnectDbContext _context;


        public CertificatesController(
            CareConnectDbContext context)
        {
            _context = context;
        }


        // ============================================
        // THƯ MỤC FILE CHỨNG CHỈ
        // PHẢI GIỐNG BÊN CAREGIVER
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
        // DANH SÁCH CHỨNG CHỈ
        // ============================================
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var danhSach =
                await _context.ChungChis
                    .AsNoTracking()
                    .Include(x =>
                        x.NguoiChamSoc)
                    .OrderBy(x =>
                        x.TrangThaiXacMinh
                            == "Chờ xác minh"
                            ? 0
                            : 1)
                    .ThenByDescending(x =>
                        x.MaChungChi)
                    .ToListAsync();


            return View(danhSach);
        }


        // ============================================
        // XEM FILE CHỨNG CHỈ
        // ============================================
        [HttpGet]
        public async Task<IActionResult> ViewFile(
            int id)
        {
            var chungChi =
                await _context.ChungChis
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x =>
                        x.MaChungChi == id);


            if (chungChi == null)
            {
                return NotFound();
            }


            if (string.IsNullOrWhiteSpace(
                chungChi.FileChungChi))
            {
                TempData["Error"] =
                    "Chứng chỉ này chưa có file đính kèm.";

                return RedirectToAction(
                    nameof(Index));
            }


            string fileName =
                Path.GetFileName(
                    chungChi.FileChungChi);


            if (fileName !=
                chungChi.FileChungChi)
            {
                return NotFound();
            }


            string filePath =
                Path.Combine(
                    LayThuMucChungChi(),
                    fileName);


            if (!System.IO.File.Exists(
                filePath))
            {
                TempData["Error"] =
                    "Không tìm thấy file chứng chỉ trên hệ thống.";

                return RedirectToAction(
                    nameof(Index));
            }


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


            return PhysicalFile(
                filePath,
                contentType);
        }


        // ============================================
        // ADMIN XÁC MINH
        // ============================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Verify(
            int id,
            string? phuongThucXacMinh)
        {
            var chungChi =
                await _context.ChungChis
                    .FirstOrDefaultAsync(x =>
                        x.MaChungChi == id);


            if (chungChi == null)
            {
                return NotFound();
            }


            if (string.IsNullOrWhiteSpace(
                chungChi.FileChungChi))
            {
                TempData["Error"] =
                    "Không thể xác minh chứng chỉ chưa có file.";

                return RedirectToAction(
                    nameof(Index));
            }


            chungChi.TrangThaiXacMinh =
                "Đã xác minh";


            chungChi.NgayXacMinh =
                DateTime.Now;


            chungChi.PhuongThucXacMinh =
                string.IsNullOrWhiteSpace(
                    phuongThucXacMinh)
                    ? "Kiểm tra hồ sơ online"
                    : phuongThucXacMinh;


            chungChi.GhiChuXacMinh =
                null;


            await _context.SaveChangesAsync();


            TempData["Success"] =
                "Đã xác minh chứng chỉ thành công.";


            return RedirectToAction(
                nameof(Index));
        }


        // ============================================
        // YÊU CẦU BỔ SUNG
        // ============================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult>
            RequestSupplement(
                int id,
                string? ghiChu)
        {
            var chungChi =
                await _context.ChungChis
                    .FirstOrDefaultAsync(x =>
                        x.MaChungChi == id);


            if (chungChi == null)
            {
                return NotFound();
            }


            if (string.IsNullOrWhiteSpace(
                ghiChu))
            {
                TempData["Error"] =
                    "Vui lòng nhập nội dung cần bổ sung.";

                return RedirectToAction(
                    nameof(Index));
            }


            chungChi.TrangThaiXacMinh =
                "Yêu cầu bổ sung";


            chungChi.NgayXacMinh =
                null;


            chungChi.PhuongThucXacMinh =
                null;


            chungChi.GhiChuXacMinh =
                ghiChu.Trim();


            await _context.SaveChangesAsync();


            TempData["Success"] =
                "Đã gửi yêu cầu bổ sung cho người chăm sóc.";


            return RedirectToAction(
                nameof(Index));
        }
        // ============================================
        // YÊU CẦU NGƯỜI CHĂM SÓC BỔ SUNG FILE
        // ============================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RequestFile(
            int id)
        {
            var chungChi =
                await _context.ChungChis
                    .Include(x => x.NguoiChamSoc)
                    .FirstOrDefaultAsync(x =>
                        x.MaChungChi == id);


            if (chungChi == null)
            {
                return NotFound();
            }


            // Đã có file thì không dùng chức năng này
            if (!string.IsNullOrWhiteSpace(
                chungChi.FileChungChi))
            {
                TempData["Error"] =
                    "Chứng chỉ này đã có file đính kèm.";

                return RedirectToAction(
                    nameof(Index));
            }


            // Không tìm được người chăm sóc
            if (chungChi.NguoiChamSoc == null)
            {
                TempData["Error"] =
                    "Không tìm thấy thông tin người chăm sóc.";

                return RedirectToAction(
                    nameof(Index));
            }


            // ========================================
            // CẬP NHẬT TRẠNG THÁI CHỨNG CHỈ
            // ========================================

            chungChi.TrangThaiXacMinh =
                "Yêu cầu bổ sung";


            chungChi.GhiChuXacMinh =
                "Vui lòng bổ sung file ảnh hoặc PDF "
                + "của chứng chỉ để quản trị viên xác minh.";


            chungChi.NgayXacMinh =
                null;


            chungChi.PhuongThucXacMinh =
                null;


            // ========================================
            // TẠO THÔNG BÁO CHO NGƯỜI CHĂM SÓC
            // ========================================
            if (!chungChi.NguoiChamSoc.MaTaiKhoan.HasValue)
            {
                TempData["Error"] =
                    "Người chăm sóc chưa liên kết với tài khoản.";

                return RedirectToAction(
                    nameof(Index));
            }
            var thongBao =
                new ThongBao
                {
                    MaTaiKhoan =
    chungChi.NguoiChamSoc.MaTaiKhoan.Value,

                    TieuDe =
                        "Yêu cầu bổ sung chứng chỉ",

                    NoiDung =
                        $"Chứng chỉ \"{chungChi.TenChungChi}\" "
                        + "chưa có file đính kèm. "
                        + "Vui lòng bổ sung ảnh hoặc PDF "
                        + "chứng chỉ để quản trị viên xác minh.",

                    DaDoc =
                        false,

                    NgayGui =
                        DateTime.Now
                };


            _context.ThongBaos.Add(
                thongBao);


            await _context.SaveChangesAsync();


            TempData["Success"] =
                $"Đã gửi yêu cầu bổ sung file cho "
                + $"{chungChi.NguoiChamSoc.HoTen}.";


            return RedirectToAction(
                nameof(Index));
        }

        // ============================================
        // KHÔNG HỢP LỆ
        // ============================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reject(
            int id,
            string? ghiChu)
        {
            var chungChi =
                await _context.ChungChis
                    .FirstOrDefaultAsync(x =>
                        x.MaChungChi == id);


            if (chungChi == null)
            {
                return NotFound();
            }


            if (string.IsNullOrWhiteSpace(
                ghiChu))
            {
                TempData["Error"] =
                    "Vui lòng nhập lý do chứng chỉ không hợp lệ.";

                return RedirectToAction(
                    nameof(Index));
            }


            chungChi.TrangThaiXacMinh =
                "Không hợp lệ";


            chungChi.NgayXacMinh =
                DateTime.Now;


            chungChi.PhuongThucXacMinh =
                null;


            chungChi.GhiChuXacMinh =
                ghiChu.Trim();


            await _context.SaveChangesAsync();


            TempData["Success"] =
                "Đã đánh dấu chứng chỉ không hợp lệ.";


            return RedirectToAction(
                nameof(Index));
        }
    }
}