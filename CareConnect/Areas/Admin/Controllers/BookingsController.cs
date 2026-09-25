using CareConnect.Data;
using CareConnect.Constants;
using CareConnect.Models;
using CareConnect.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using System.Text;
namespace CareConnect.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Quản trị viên")]
    public class BookingsController : Controller
    {
        private readonly CareConnectDbContext _context;

        public BookingsController(
            CareConnectDbContext context)
        {
            _context = context;
        }
        private static string LayKhuVucTuDiaChi(
            string? diaChi)
        {
            if (string.IsNullOrWhiteSpace(diaChi))
            {
                return "Chưa cập nhật";
            }

            string[] cacPhan =
                diaChi.Split(
                    ',',
                    StringSplitOptions.RemoveEmptyEntries);

            foreach (string phan in cacPhan)
            {
                string giaTri = phan.Trim();

                if (giaTri.StartsWith(
                        "Quận ",
                        StringComparison.OrdinalIgnoreCase))
                {
                    return giaTri;
                }

                if (giaTri.StartsWith(
                        "Huyện ",
                        StringComparison.OrdinalIgnoreCase))
                {
                    return giaTri;
                }

                if (giaTri.Contains(
                        "Thủ Đức",
                        StringComparison.OrdinalIgnoreCase))
                {
                    return "TP. Thủ Đức";
                }
            }

            return "Khu vực khác";
        }
        // ============================================================
        // THUẬT TOÁN GỢI Ý NGƯỜI CHĂM SÓC ĐA TIÊU CHÍ
        // ============================================================

        private static double GioiHanDiem(double diem)
        {
            if (diem < 0)
            {
                return 0;
            }

            if (diem > 100)
            {
                return 100;
            }

            return diem;
        }


        // ============================================================
        // CHUẨN HÓA CHUỖI TIẾNG VIỆT
        //
        // Ví dụ:
        // "Quận Gò Vấp" -> "quan go vap"
        // "Chăm sóc Alzheimer" -> "cham soc alzheimer"
        // ============================================================

        private static string ChuanHoaVanBan(
            string? vanBan)
        {
            if (string.IsNullOrWhiteSpace(vanBan))
            {
                return string.Empty;
            }

            string normalized =
                vanBan
                    .Trim()
                    .ToLowerInvariant()
                    .Normalize(
                        NormalizationForm.FormD);

            var builder =
                new StringBuilder();

            foreach (char kyTu in normalized)
            {
                UnicodeCategory category =
                    CharUnicodeInfo.GetUnicodeCategory(
                        kyTu);

                if (category ==
                    UnicodeCategory.NonSpacingMark)
                {
                    continue;
                }

                if (char.IsLetterOrDigit(kyTu))
                {
                    builder.Append(kyTu);
                }
                else
                {
                    builder.Append(' ');
                }
            }

            return string.Join(
                " ",
                builder
                    .ToString()
                    .Normalize(
                        NormalizationForm.FormC)
                    .Split(
                        ' ',
                        StringSplitOptions
                            .RemoveEmptyEntries));
        }


        // ============================================================
        // TÁCH TỪ KHÓA CHUYÊN MÔN
        // ============================================================

        private static HashSet<string> TachTuKhoa(
            string? vanBan)
        {
            string daChuanHoa =
                ChuanHoaVanBan(vanBan);

            if (string.IsNullOrWhiteSpace(
                    daChuanHoa))
            {
                return new HashSet<string>();
            }

            /*
             * Các từ quá phổ biến không giúp phân biệt
             * chuyên môn của người chăm sóc.
             */
            string[] tuDung =
            {
        "cham",
        "soc",
        "nguoi",
        "dich",
        "vu",
        "tai",
        "nha",
        "cho",
        "cac",
        "va",
        "theo",
        "ho",
        "tro",
        "can",
        "duoc",
        "benh",
        "nhan"
    };

            var tapTuDung =
                tuDung.ToHashSet();

            return daChuanHoa
                .Split(
                    ' ',
                    StringSplitOptions
                        .RemoveEmptyEntries)
                .Where(x =>
                    x.Length >= 3
                    &&
                    !tapTuDung.Contains(x))
                .ToHashSet();
        }


        // ============================================================
        // 1. ĐIỂM KHU VỰC
        //
        // 100 = cùng khu vực
        // 35  = khác khu vực
        //
        // Nếu Admin tắt "Ưu tiên cùng khu vực"
        // thì tất cả caregiver nhận 100 điểm trung tính.
        // ============================================================

        private static double TinhDiemKhuVuc(
            string? diaChiChamSoc,
            string? khuVucNguoiChamSoc,
            bool uuTienCungKhuVuc)
        {
            if (!uuTienCungKhuVuc)
            {
                return 100;
            }

            if (string.IsNullOrWhiteSpace(
                    diaChiChamSoc)
                ||
                string.IsNullOrWhiteSpace(
                    khuVucNguoiChamSoc))
            {
                return 50;
            }

            string diaChi =
                ChuanHoaVanBan(
                    diaChiChamSoc);

            string khuVuc =
                ChuanHoaVanBan(
                    khuVucNguoiChamSoc);

            if (string.IsNullOrWhiteSpace(
                    diaChi)
                ||
                string.IsNullOrWhiteSpace(
                    khuVuc))
            {
                return 50;
            }

            /*
             * Trường hợp caregiver chỉ hoạt động
             * ở một khu vực.
             */
            if (diaChi.Contains(khuVuc))
            {
                return 100;
            }

            /*
             * Trường hợp caregiver lưu nhiều khu vực:
             * "Gò Vấp, Bình Thạnh, Phú Nhuận"
             */
            string[] cacKhuVuc =
                khuVucNguoiChamSoc!
                    .Split(
                        new[]
                        {
                    ',',
                    ';',
                    '|'
                        },
                        StringSplitOptions
                            .RemoveEmptyEntries);

            foreach (string item in cacKhuVuc)
            {
                string khuVucCon =
                    ChuanHoaVanBan(item);

                if (!string.IsNullOrWhiteSpace(
                        khuVucCon)
                    &&
                    diaChi.Contains(khuVucCon))
                {
                    return 100;
                }
            }

            /*
             * So sánh khu vực được tách từ địa chỉ.
             */
            string khuVucLich =
                ChuanHoaVanBan(
                    LayKhuVucTuDiaChi(
                        diaChiChamSoc));

            if (!string.IsNullOrWhiteSpace(
                    khuVucLich)
                &&
                khuVuc.Contains(khuVucLich))
            {
                return 100;
            }

            return 35;
        }


        // ============================================================
        // 2. ĐIỂM CHUYÊN MÔN
        //
        // So sánh:
        // - Tên dịch vụ
        // - Mô tả dịch vụ
        // - Tình trạng sức khỏe
        // - Tiền sử bệnh
        // - Ghi chú
        //
        // với:
        // - Chuyên môn caregiver
        // - Bằng cấp
        // - Giới thiệu
        // ============================================================

        private static double TinhDiemChuyenMon(
            string? yeuCauChamSoc,
            string? thongTinChuyenMon)
        {
            HashSet<string> tuKhoaYeuCau =
                TachTuKhoa(
                    yeuCauChamSoc);

            HashSet<string> tuKhoaCaregiver =
                TachTuKhoa(
                    thongTinChuyenMon);

            /*
             * Không đủ thông tin để so sánh
             * => cho điểm trung tính.
             */
            if (tuKhoaYeuCau.Count == 0
                ||
                tuKhoaCaregiver.Count == 0)
            {
                return 50;
            }

            int soTuTrung =
                tuKhoaYeuCau
                    .Count(x =>
                        tuKhoaCaregiver
                            .Contains(x));

            if (soTuTrung == 0)
            {
                return 35;
            }

            double tyLe =
                (double)soTuTrung
                /
                tuKhoaYeuCau.Count;

            /*
             * Có ít nhất một từ chuyên môn trùng
             * thì không để điểm quá thấp.
             */
            double diem =
                Math.Max(
                    55,
                    tyLe * 100);

            return GioiHanDiem(diem);
        }


        // ============================================================
        // 3. ĐIỂM KINH NGHIỆM
        //
        // 0 năm   = 0 điểm
        // 5 năm   = 50 điểm
        // 10 năm+ = 100 điểm
        // ============================================================

        private static double TinhDiemKinhNghiem(
            int kinhNghiem)
        {
            if (kinhNghiem <= 0)
            {
                return 0;
            }

            double diem =
                kinhNghiem
                /
                10.0
                *
                100.0;

            return GioiHanDiem(diem);
        }


        // ============================================================
        // 4. ĐIỂM ĐÁNH GIÁ
        //
        // 5 sao = 100 điểm
        // 4 sao = 80 điểm
        // 3 sao = 60 điểm
        // ============================================================

        private static double TinhDiemDanhGia(
            double danhGia)
        {
            if (danhGia <= 0)
            {
                return 0;
            }

            return GioiHanDiem(
                danhGia
                /
                5.0
                *
                100.0);
        }


        // ============================================================
        // 5. ĐIỂM MỨC GIÁ
        //
        // So sánh GiaTheoGio của caregiver với
        // giá dịch vụ quy đổi theo giờ.
        //
        // Giá caregiver <= giá mục tiêu:
        //      100 điểm
        //
        // Giá cao hơn:
        //      giảm dần theo % vượt mức.
        // ============================================================

        private static double TinhDiemMucGia(
            decimal giaTheoGio,
            decimal giaMucTieuTheoGio)
        {
            if (giaTheoGio <= 0
                ||
                giaMucTieuTheoGio <= 0)
            {
                return 50;
            }

            if (giaTheoGio
                <= giaMucTieuTheoGio)
            {
                return 100;
            }

            decimal tyLeVuotGia =
                (
                    giaTheoGio
                    -
                    giaMucTieuTheoGio
                )
                /
                giaMucTieuTheoGio;

            double diem =
                100
                -
                (double)(
                    tyLeVuotGia
                    *
                    100m);

            return GioiHanDiem(diem);
        }


        // ============================================================
        // 6. ĐIỂM LỊCH TRỐNG
        //
        // Người không có lịch nào trong ngày
        // sẽ được ưu tiên hơn người đã có nhiều lịch.
        //
        // Lưu ý:
        // caregiver TRÙNG GIỜ đã bị loại hoàn toàn
        // trước khi tới bước chấm điểm này.
        // ============================================================

        private static double TinhDiemLichTrong(
            int soLichTrongNgay)
        {
            return soLichTrongNgay switch
            {
                <= 0 => 100,
                1 => 85,
                2 => 70,
                3 => 55,
                4 => 40,
                _ => 25
            };
        }


        // ============================================================
        // TẠO LÝ DO GIẢI THÍCH GỢI Ý
        // ============================================================

        private static string TaoLyDoGoiY(
            double diemKhuVuc,
            double diemChuyenMon,
            double diemKinhNghiem,
            double diemDanhGia,
            double diemMucGia,
            double diemLichTrong)
        {
            var lyDo =
                new List<string>();

            if (diemKhuVuc >= 90)
            {
                lyDo.Add(
                    "khu vực phù hợp");
            }

            if (diemChuyenMon >= 75)
            {
                lyDo.Add(
                    "chuyên môn phù hợp");
            }

            if (diemKinhNghiem >= 70)
            {
                lyDo.Add(
                    "nhiều kinh nghiệm");
            }

            if (diemDanhGia >= 80)
            {
                lyDo.Add(
                    "đánh giá tốt");
            }

            if (diemMucGia >= 85)
            {
                lyDo.Add(
                    "mức giá phù hợp");
            }

            if (diemLichTrong >= 85)
            {
                lyDo.Add(
                    "lịch làm việc còn thoáng");
            }

            if (lyDo.Count == 0)
            {
                return
                    "phù hợp theo tổng hợp nhiều tiêu chí";
            }

            return string.Join(
                " • ",
                lyDo.Take(3));
        }
        [HttpGet]
        public async Task<IActionResult> Assign(int id)
        {
            // ========================================================
            // 1. LẤY LỊCH CẦN PHÂN CÔNG
            // ========================================================

            var lich =
                await _context.DatLichs
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x =>
                        x.MaDatLich == id);

            if (lich == null)
            {
                return NotFound();
            }


            if (lich.TrangThai != "Chờ xác nhận")
            {
                TempData["Error"] =
                    "Chỉ có thể phân công lịch đang chờ xác nhận.";

                return RedirectToAction(
                    nameof(Index));
            }


            if (!lich.NgayChamSoc.HasValue
                ||
                !lich.GioBatDau.HasValue
                ||
                !lich.GioKetThuc.HasValue)
            {
                TempData["Error"] =
                    "Lịch chưa có đầy đủ ngày và khung giờ chăm sóc.";

                return RedirectToAction(
                    nameof(Index));
            }


            DateTime ngayChamSoc =
                lich.NgayChamSoc.Value.Date;

            DateTime ngayKeTiep =
                ngayChamSoc.AddDays(1);

            TimeSpan gioBatDau =
                lich.GioBatDau.Value;

            TimeSpan gioKetThuc =
                lich.GioKetThuc.Value;


            if (gioKetThuc <= gioBatDau)
            {
                TempData["Error"] =
                    "Giờ kết thúc phải lớn hơn giờ bắt đầu.";

                return RedirectToAction(
                    nameof(Index));
            }


            // ========================================================
            // 2. LẤY THÔNG TIN NGHIỆP VỤ LIÊN QUAN
            // ========================================================

            var khachHang =
                await _context.KhachHangs
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x =>
                        x.MaKhachHang
                            == lich.MaKhachHang);


            var benhNhan =
                await _context.BenhNhans
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x =>
                        x.MaBenhNhan
                            == lich.MaBenhNhan);


            var dichVu =
                await _context.DichVus
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x =>
                        x.MaDichVu
                            == lich.MaDichVu);


            // ========================================================
            // 3. LẤY CẤU HÌNH GỢI Ý ĐANG ÁP DỤNG
            // ========================================================

            CauHinhGoiY? cauHinh =
    await _context.CauHinhGoiYs
        .AsNoTracking()
        .OrderByDescending(x =>
            x.DangApDung)
        .ThenByDescending(x =>
            x.NgayCapNhat)
        .FirstOrDefaultAsync();


            /*
             * Nếu database chưa có cấu hình,
             * dùng cấu hình mặc định.
             *
             * Không lưu xuống DB tại đây.
             */
            if (cauHinh == null)
            {
                cauHinh =
                    new CauHinhGoiY
                    {
                        TenCauHinh =
                            "Cấu hình gợi ý mặc định",

                        TrongSoKhuVuc = 25,

                        TrongSoChuyenMon = 25,

                        TrongSoKinhNghiem = 15,

                        TrongSoDanhGia = 20,

                        TrongSoMucGia = 10,

                        TrongSoLichTrong = 5,

                        SoLuongGoiY = 5,

                        DiemDanhGiaToiThieu = 3,

                        KinhNghiemToiThieu = 0,

                        ChiGoiYNguoiDaDuyet = true,

                        UuTienCungKhuVuc = true,

                        DangApDung = true,

                        NgayCapNhat =
                            DateTime.Now
                    };
            }


            // ========================================================
            // 4. LẤY DANH SÁCH TÀI KHOẢN BỊ KHÓA
            // ========================================================

            var taiKhoanBiKhoa =
                await _context.TaiKhoans
                    .AsNoTracking()
                    .Where(x =>
                        !x.TrangThai)
                    .Select(x =>
                        x.MaTaiKhoan)
                    .ToListAsync();


            // ========================================================
            // 5. LẤY CAREGIVER CÓ THỂ ĐƯỢC PHÂN CÔNG
            //
            // Chỉ lấy:
            // - Đang hoạt động / Đã duyệt
            // - Tài khoản không bị khóa
            // - Đủ kinh nghiệm tối thiểu
            // ========================================================

            var danhSachNguoiChamSocGoc =
    await _context.NguoiChamSocs
        .AsNoTracking()
        .ToListAsync();
            if (cauHinh.ChiGoiYNguoiDaDuyet)
            {
                danhSachNguoiChamSocGoc =
                    danhSachNguoiChamSocGoc
                        .Where(x =>
                            x.TrangThai == "Đang hoạt động"
            ||
            x.TrangThai == "Đã duyệt")
                        .ToList();
            }
            else
            {
                /*
                 * Dù Admin tắt điều kiện "chỉ hồ sơ đã duyệt",
                 * vẫn không bao giờ đưa hồ sơ bị từ chối vào
                 * danh sách phân công.
                 */
                danhSachNguoiChamSocGoc =
                    danhSachNguoiChamSocGoc
                        .Where(x =>
                            x.TrangThai != "Từ chối")
                        .ToList();
            }

            danhSachNguoiChamSocGoc =
    danhSachNguoiChamSocGoc
        .Where(x =>
            !x.MaTaiKhoan.HasValue
            ||
            !taiKhoanBiKhoa.Contains(
                x.MaTaiKhoan.Value))
        .ToList();


            // ========================================================
            // 6. LOẠI CAREGIVER BỊ TRÙNG ĐÚNG KHUNG GIỜ
            // ========================================================
            var nguoiChamSocDaTuChoi =
    await _context.TuChoiLichs
        .AsNoTracking()
        .Where(x =>
            x.MaDatLich
                == lich.MaDatLich)
        .Select(x =>
            x.MaNguoiChamSoc)
        .Distinct()
        .ToListAsync();


            danhSachNguoiChamSocGoc =
                danhSachNguoiChamSocGoc
                    .Where(x =>
                        !nguoiChamSocDaTuChoi.Contains(
                            x.MaNguoiChamSoc))
                    .ToList();
            var nguoiChamSocBiTrungLich =
                await _context.DatLichs
                    .AsNoTracking()
                    .Where(x =>
                        x.MaDatLich
                            != lich.MaDatLich
                        &&
                        x.MaNguoiChamSoc.HasValue
                        &&
                        x.NgayChamSoc.HasValue
                        &&
                        x.GioBatDau.HasValue
                        &&
                        x.GioKetThuc.HasValue
                        &&
                        x.NgayChamSoc.Value
                            >= ngayChamSoc
                        &&
                        x.NgayChamSoc.Value
                            < ngayKeTiep
                        &&
                        x.TrangThai
                            != "Đã hủy"
                        &&
                        x.TrangThai
                            != "Đã hoàn thành"
                        &&
                        x.GioBatDau.Value
                            < gioKetThuc
                        &&
                        x.GioKetThuc.Value
                            > gioBatDau)
                    .Select(x =>
                        x.MaNguoiChamSoc!.Value)
                    .Distinct()
                    .ToListAsync();


            danhSachNguoiChamSocGoc =
                danhSachNguoiChamSocGoc
                    .Where(x =>
                        !nguoiChamSocBiTrungLich
                            .Contains(
                                x.MaNguoiChamSoc))
                    .ToList();
            if (!cauHinh.DangApDung)
            {
                var danhSachKhongDungThuatToan =
                    danhSachNguoiChamSocGoc
                        .OrderByDescending(x =>
                            x.DanhGia ?? 0)
                        .ThenByDescending(x =>
                            x.KinhNghiem ?? 0)
                        .ThenBy(x =>
                            x.HoTen)
                        .Select((x, index) =>
                            new AdminAssignableCaregiverViewModel
                            {
                                MaNguoiChamSoc =
                                    x.MaNguoiChamSoc,

                                HoTen =
                                    x.HoTen
                                    ?? "Người chăm sóc",

                                ChuyenMon =
                                    x.ChuyenMon
                                    ?? "Chưa cập nhật",

                                KhuVucHoatDong =
                                    x.KhuVucHoatDong
                                    ?? "Chưa cập nhật",

                                KinhNghiem =
                                    x.KinhNghiem ?? 0,

                                GiaTheoGio =
                                    x.GiaTheoGio ?? 0,

                                DanhGia =
                                    x.DanhGia ?? 0,

                                Email =
                                    x.Email
                                    ?? string.Empty,

                                SoDienThoai =
                                    x.SoDienThoai
                                    ?? string.Empty,

                                ThuHang =
                                    index + 1,

                                LaGoiY = false,

                                DiemPhuHop = 0,

                                LyDoGoiY =
                                    "Thuật toán gợi ý đang tạm dừng."
                            })
                        .ToList();


                var modelKhongDungThuatToan =
                    new AdminAssignBookingViewModel
                    {
                        MaDatLich =
                            lich.MaDatLich,

                        MaNguoiChamSoc =
                            lich.MaNguoiChamSoc,

                        TenKhachHang =
                            khachHang?.HoTen
                            ?? "Khách hàng",

                        TenBenhNhan =
                            benhNhan?.HoTen
                            ?? "Người được chăm sóc",

                        TenDichVu =
                            dichVu?.TenDichVu
                            ?? "Dịch vụ chăm sóc",

                        KhuVuc =
                            string.IsNullOrWhiteSpace(
                                lich.DiaChiChamSoc)

                                ? benhNhan?.DiaChi
                                    ?? "Chưa cập nhật"

                                : lich.DiaChiChamSoc,

                        NgayChamSoc =
                            ngayChamSoc,

                        GioBatDau =
                            gioBatDau,

                        GioKetThuc =
                            gioKetThuc,

                        TongTien =
                            lich.TongTien ?? 0,

                        DangApDungGoiY = false,

                        TenCauHinhGoiY =
                            cauHinh.TenCauHinh,

                        SoLuongGoiY =
                            cauHinh.SoLuongGoiY,

                        TrongSoKhuVuc =
                            cauHinh.TrongSoKhuVuc,

                        TrongSoChuyenMon =
                            cauHinh.TrongSoChuyenMon,

                        TrongSoKinhNghiem =
                            cauHinh.TrongSoKinhNghiem,

                        TrongSoDanhGia =
                            cauHinh.TrongSoDanhGia,

                        TrongSoMucGia =
                            cauHinh.TrongSoMucGia,

                        TrongSoLichTrong =
                            cauHinh.TrongSoLichTrong,

                        DanhSachNguoiChamSoc =
                            danhSachKhongDungThuatToan
                    };


                return View(
                    modelKhongDungThuatToan);
            }

            // ========================================================
            // 7. ĐẾM SỐ LỊCH CỦA TỪNG CAREGIVER TRONG NGÀY
            //
            // Dùng cho tiêu chí "Lịch trống".
            // ========================================================

            var soLichTrongNgay =
                await _context.DatLichs
                    .AsNoTracking()
                    .Where(x =>
                        x.MaNguoiChamSoc.HasValue
                        &&
                        x.NgayChamSoc.HasValue
                        &&
                        x.NgayChamSoc.Value
                            >= ngayChamSoc
                        &&
                        x.NgayChamSoc.Value
                            < ngayKeTiep
                        &&
                        x.TrangThai
                            != "Đã hủy")
                    .GroupBy(x =>
                        x.MaNguoiChamSoc!.Value)
                    .Select(g =>
                        new
                        {
                            MaNguoiChamSoc =
                                g.Key,

                            SoLich =
                                g.Count()
                        })
                    .ToDictionaryAsync(
                        x =>
                            x.MaNguoiChamSoc,

                        x =>
                            x.SoLich);


            // ========================================================
            // 8. TÍNH ĐÁNH GIÁ THỰC TẾ TỪ BẢNG DanhGia
            //
            // DanhGia
            //      ↓
            // DatLich
            //      ↓
            // MaNguoiChamSoc
            // ========================================================

            var danhGiaThucTe =
                await (
                    from danhGia
                        in _context.DanhGias
                            .AsNoTracking()

                    join datLich
                        in _context.DatLichs
                            .AsNoTracking()

                        on danhGia.MaDatLich
                        equals datLich.MaDatLich

                    where
                        datLich.MaNguoiChamSoc
                            .HasValue

                    group danhGia
                        by datLich.MaNguoiChamSoc!.Value
                        into nhom

                    select new
                    {
                        MaNguoiChamSoc =
                            nhom.Key,

                        DiemTrungBinh =
                            nhom.Average(x =>
                                (double)x.SoSao)
                    }
                )
                .ToDictionaryAsync(
                    x =>
                        x.MaNguoiChamSoc,

                    x =>
                        x.DiemTrungBinh);


            // ========================================================
            // 9. CHUẨN BỊ DỮ LIỆU CHO TIÊU CHÍ KHU VỰC
            // ========================================================

            string diaChiChamSoc =
                !string.IsNullOrWhiteSpace(
                    lich.DiaChiChamSoc)

                    ? lich.DiaChiChamSoc!

                    : benhNhan?.DiaChi
                        ?? string.Empty;


            // ========================================================
            // 10. CHUẨN BỊ DỮ LIỆU CHO TIÊU CHÍ CHUYÊN MÔN
            //
            // Ghép thông tin của yêu cầu chăm sóc
            // để thuật toán hiểu người bệnh cần gì.
            // ========================================================

            string yeuCauChamSoc =
                string.Join(
                    " ",
                    new[]
                    {
                dichVu?.TenDichVu,
                dichVu?.MoTa,
                benhNhan?.TinhTrangSucKhoe,
                benhNhan?.TienSuBenh,
                benhNhan?.GhiChu,
                lich.GhiChu
                    }
                    .Where(x =>
                        !string.IsNullOrWhiteSpace(x)));


            // ========================================================
            // 11. QUY ĐỔI GIÁ DỊCH VỤ THÀNH GIÁ / GIỜ
            //
            // Ví dụ:
            // Giá dịch vụ: 300.000
            // Thời lượng: 120 phút
            //
            // => giá mục tiêu / giờ = 150.000
            // ========================================================

            decimal giaMucTieuTheoGio = 0m;

            if (dichVu != null
                &&
                dichVu.ThoiLuong > 0
                &&
                dichVu.Gia > 0)
            {
                giaMucTieuTheoGio =
                    dichVu.Gia
                    /
                    dichVu.ThoiLuong
                    *
                    60m;
            }


            // ========================================================
            // 12. TÍNH TỔNG TRỌNG SỐ
            // ========================================================

            int tongTrongSo =
                cauHinh.TrongSoKhuVuc
                +
                cauHinh.TrongSoChuyenMon
                +
                cauHinh.TrongSoKinhNghiem
                +
                cauHinh.TrongSoDanhGia
                +
                cauHinh.TrongSoMucGia
                +
                cauHinh.TrongSoLichTrong;


            /*
             * Phòng trường hợp dữ liệu cấu hình lỗi.
             */
            if (tongTrongSo <= 0)
            {
                tongTrongSo = 100;
            }


            // ========================================================
            // 13. CHẤM ĐIỂM TỪNG NGƯỜI CHĂM SÓC
            // ========================================================

            var danhSachDaChamDiem =
                new List<
                    AdminAssignableCaregiverViewModel>();


            foreach (
                var nguoiChamSoc
                in danhSachNguoiChamSocGoc)
            {
                // ----------------------------------------------------
                // ĐÁNH GIÁ THỰC TẾ
                // ----------------------------------------------------

                bool coDanhGiaThucTe =
                    danhGiaThucTe.TryGetValue(
                        nguoiChamSoc.MaNguoiChamSoc,
                        out double diemDanhGiaThucTe);


                double diemSao =
                    coDanhGiaThucTe

                        ? diemDanhGiaThucTe

                        : nguoiChamSoc.DanhGia
                            ?? 0d;
                bool datKinhNghiem =
    (nguoiChamSoc.KinhNghiem ?? 0)
    >= cauHinh.KinhNghiemToiThieu;


                bool datDanhGia =
                    diemSao
                    >= (double)cauHinh.DiemDanhGiaToiThieu;


                bool datNguongGoiY =
                    datKinhNghiem
                    &&
                    datDanhGia;


                var ghiChuNguong =
                    new List<string>();


                if (!datKinhNghiem)
                {
                    ghiChuNguong.Add(
                        $"Kinh nghiệm dưới {cauHinh.KinhNghiemToiThieu} năm");
                }


                if (!datDanhGia)
                {
                    ghiChuNguong.Add(
                        $"Đánh giá dưới {cauHinh.DiemDanhGiaToiThieu:0.0} sao");
                }


                string ghiChuNguongGoiY =
                    ghiChuNguong.Count == 0
                        ? "Đạt điều kiện gợi ý"
                        : string.Join(
                            " • ",
                            ghiChuNguong);

                /*
                 * Lọc theo điểm đánh giá tối thiểu
                 * trong CauHinhGoiY.
                 */


                // ----------------------------------------------------
                // SỐ LỊCH TRONG NGÀY
                // ----------------------------------------------------

                int soLich =
                    soLichTrongNgay
                        .TryGetValue(
                            nguoiChamSoc
                                .MaNguoiChamSoc,
                            out int soLichTimThay)

                        ? soLichTimThay

                        : 0;


                // ----------------------------------------------------
                // THÔNG TIN CHUYÊN MÔN CỦA CAREGIVER
                // ----------------------------------------------------

                string thongTinChuyenMon =
                    string.Join(
                        " ",
                        new[]
                        {
                    nguoiChamSoc.ChuyenMon,
                    nguoiChamSoc.BangCap,
                    nguoiChamSoc.GioiThieu
                        }
                        .Where(x =>
                            !string.IsNullOrWhiteSpace(x)));


                // ----------------------------------------------------
                // 6 ĐIỂM THÀNH PHẦN
                // ----------------------------------------------------

                double diemKhuVuc =
                    TinhDiemKhuVuc(
                        diaChiChamSoc,
                        nguoiChamSoc
                            .KhuVucHoatDong,
                        cauHinh
                            .UuTienCungKhuVuc);


                double diemChuyenMon =
                    TinhDiemChuyenMon(
                        yeuCauChamSoc,
                        thongTinChuyenMon);


                double diemKinhNghiem =
                    TinhDiemKinhNghiem(
                        nguoiChamSoc
                            .KinhNghiem
                        ?? 0);


                double diemDanhGia =
                    TinhDiemDanhGia(
                        diemSao);


                double diemMucGia =
                    TinhDiemMucGia(
                        nguoiChamSoc
                            .GiaTheoGio
                        ?? 0m,
                        giaMucTieuTheoGio);


                double diemLichTrong =
                    TinhDiemLichTrong(
                        soLich);


                // ----------------------------------------------------
                // ĐIỂM TỔNG CÓ TRỌNG SỐ
                // ----------------------------------------------------

                double diemPhuHop =
                    (
                        diemKhuVuc
                            *
                            cauHinh
                                .TrongSoKhuVuc

                        +

                        diemChuyenMon
                            *
                            cauHinh
                                .TrongSoChuyenMon

                        +

                        diemKinhNghiem
                            *
                            cauHinh
                                .TrongSoKinhNghiem

                        +

                        diemDanhGia
                            *
                            cauHinh
                                .TrongSoDanhGia

                        +

                        diemMucGia
                            *
                            cauHinh
                                .TrongSoMucGia

                        +

                        diemLichTrong
                            *
                            cauHinh
                                .TrongSoLichTrong
                    )
                    /
                    tongTrongSo;


                diemPhuHop =
                    Math.Round(
                        GioiHanDiem(
                            diemPhuHop),
                        1);


                // ----------------------------------------------------
                // THÊM VÀO DANH SÁCH KẾT QUẢ
                // ----------------------------------------------------

                danhSachDaChamDiem.Add(
                    new AdminAssignableCaregiverViewModel
                    {
                        MaNguoiChamSoc =
                            nguoiChamSoc
                                .MaNguoiChamSoc,

                        HoTen =
                            nguoiChamSoc.HoTen
                            ?? "Người chăm sóc",

                        ChuyenMon =
                            nguoiChamSoc.ChuyenMon
                            ?? "Chưa cập nhật",

                        KhuVucHoatDong =
                            nguoiChamSoc
                                .KhuVucHoatDong
                            ?? "Chưa cập nhật",

                        KinhNghiem =
                            nguoiChamSoc
                                .KinhNghiem
                            ?? 0,

                        GiaTheoGio =
                            nguoiChamSoc
                                .GiaTheoGio
                            ?? 0m,

                        DanhGia =
                            Math.Round(
                                diemSao,
                                1),

                        CoDanhGiaThucTe =
                            coDanhGiaThucTe,

                        Email =
                            nguoiChamSoc.Email
                            ?? string.Empty,

                        SoDienThoai =
                            nguoiChamSoc
                                .SoDienThoai
                            ?? string.Empty,

                        DiemKhuVuc =
                            Math.Round(
                                diemKhuVuc,
                                1),

                        DiemChuyenMon =
                            Math.Round(
                                diemChuyenMon,
                                1),

                        DiemKinhNghiem =
                            Math.Round(
                                diemKinhNghiem,
                                1),

                        DiemDanhGia =
                            Math.Round(
                                diemDanhGia,
                                1),

                        DiemMucGia =
                            Math.Round(
                                diemMucGia,
                                1),

                        DiemLichTrong =
                            Math.Round(
                                diemLichTrong,
                                1),

                        SoLichTrongNgay =
                            soLich,

                        DiemPhuHop =
    diemPhuHop,

                        DatNguongGoiY =
    datNguongGoiY,

                        GhiChuNguongGoiY =
    ghiChuNguongGoiY,

                        LyDoGoiY =
    TaoLyDoGoiY(
        diemKhuVuc,
        diemChuyenMon,
        diemKinhNghiem,
        diemDanhGia,
        diemMucGia,
        diemLichTrong)
                    });
            }


            // ========================================================
            // 14. XẾP HẠNG
            //
            // Ưu tiên:
            // 1. Điểm phù hợp
            // 2. Điểm đánh giá
            // 3. Kinh nghiệm
            // 4. Họ tên
            // ========================================================

            danhSachDaChamDiem =
    danhSachDaChamDiem
        .OrderByDescending(x =>
            x.DatNguongGoiY)

        .ThenByDescending(x =>
            x.DiemPhuHop)

        .ThenByDescending(x =>
            x.DanhGia)

        .ThenByDescending(x =>
            x.KinhNghiem)

        .ThenBy(x =>
            x.HoTen)

        .ToList();


            // ========================================================
            // 15. ĐÁNH SỐ THỨ HẠNG + TOP GỢI Ý
            // ========================================================

            int soLuongGoiY =
                Math.Max(
                    1,
                    cauHinh.SoLuongGoiY);


            int thuHangGoiY = 1;


            foreach (var item in danhSachDaChamDiem)
            {
                if (item.DatNguongGoiY
                    &&
                    thuHangGoiY <= soLuongGoiY)
                {
                    item.ThuHang =
                        thuHangGoiY;

                    item.LaGoiY = true;

                    thuHangGoiY++;
                }
                else
                {
                    item.ThuHang = 0;

                    item.LaGoiY = false;
                }
            }


            // ========================================================
            // 16. TẠO VIEWMODEL
            // ========================================================

            var model =
                new AdminAssignBookingViewModel
                {
                    MaDatLich =
                        lich.MaDatLich,

                    MaNguoiChamSoc =
                        lich.MaNguoiChamSoc,

                    TenKhachHang =
                        khachHang?.HoTen
                        ?? "Khách hàng",

                    TenBenhNhan =
                        benhNhan?.HoTen
                        ?? "Người được chăm sóc",

                    TenDichVu =
                        dichVu?.TenDichVu
                        ?? "Dịch vụ chăm sóc",

                    KhuVuc =
                        string.IsNullOrWhiteSpace(
                            diaChiChamSoc)

                            ? "Chưa cập nhật"

                            : diaChiChamSoc,

                    NgayChamSoc =
                        ngayChamSoc,

                    GioBatDau =
                        gioBatDau,

                    GioKetThuc =
                        gioKetThuc,

                    TongTien =
                        lich.TongTien
                        ?? 0m,


                    // =================================================
                    // THÔNG TIN THUẬT TOÁN
                    // =================================================

                    DangApDungGoiY =
                        cauHinh.DangApDung,

                    TenCauHinhGoiY =
                        cauHinh.TenCauHinh,

                    SoLuongGoiY =
                        soLuongGoiY,

                    TrongSoKhuVuc =
                        cauHinh.TrongSoKhuVuc,

                    TrongSoChuyenMon =
                        cauHinh.TrongSoChuyenMon,

                    TrongSoKinhNghiem =
                        cauHinh.TrongSoKinhNghiem,

                    TrongSoDanhGia =
                        cauHinh.TrongSoDanhGia,

                    TrongSoMucGia =
                        cauHinh.TrongSoMucGia,

                    TrongSoLichTrong =
                        cauHinh.TrongSoLichTrong,


                    DanhSachNguoiChamSoc =
                        danhSachDaChamDiem
                };


            return View(model);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Assign(
    AdminAssignBookingViewModel model)
        {
            // ========================================================
            // 1. KIỂM TRA LỊCH
            // ========================================================

            var lich =
                await _context.DatLichs
                    .FirstOrDefaultAsync(x =>
                        x.MaDatLich == model.MaDatLich);

            if (lich == null)
            {
                return NotFound();
            }


            if (lich.TrangThai != "Chờ xác nhận")
            {
                TempData["Error"] =
                    "Lịch này không còn ở trạng thái chờ xác nhận.";

                return RedirectToAction(
                    nameof(Index));
            }


            if (!lich.NgayChamSoc.HasValue
                ||
                !lich.GioBatDau.HasValue
                ||
                !lich.GioKetThuc.HasValue)
            {
                TempData["Error"] =
                    "Lịch chưa có đầy đủ ngày và khung giờ chăm sóc.";

                return RedirectToAction(
                    nameof(Index));
            }


            DateTime ngayChamSoc =
                lich.NgayChamSoc.Value.Date;

            DateTime ngayKeTiep =
                ngayChamSoc.AddDays(1);

            TimeSpan gioBatDau =
                lich.GioBatDau.Value;

            TimeSpan gioKetThuc =
                lich.GioKetThuc.Value;


            if (gioKetThuc <= gioBatDau)
            {
                TempData["Error"] =
                    "Giờ kết thúc phải lớn hơn giờ bắt đầu.";

                return RedirectToAction(
                    nameof(Index));
            }


            // ========================================================
            // 2. PHẢI CHỌN NGƯỜI CHĂM SÓC
            // ========================================================

            if (!model.MaNguoiChamSoc.HasValue)
            {
                TempData["Error"] =
                    "Vui lòng chọn người chăm sóc.";

                return RedirectToAction(
                    nameof(Assign),
                    new
                    {
                        id = model.MaDatLich
                    });
            }


            // ========================================================
            // 3. LẤY NGƯỜI CHĂM SÓC
            // ========================================================

            var nguoiChamSoc =
                await _context.NguoiChamSocs
                    .FirstOrDefaultAsync(x =>
                        x.MaNguoiChamSoc
                            == model.MaNguoiChamSoc.Value);

            if (nguoiChamSoc == null)
            {
                TempData["Error"] =
                    "Không tìm thấy người chăm sóc.";

                return RedirectToAction(
                    nameof(Assign),
                    new
                    {
                        id = model.MaDatLich
                    });
            }

            bool daTuChoiLichNay =
    await _context.TuChoiLichs
        .AsNoTracking()
        .AnyAsync(x =>
            x.MaDatLich
                == lich.MaDatLich
            &&
            x.MaNguoiChamSoc
                == nguoiChamSoc.MaNguoiChamSoc);


            if (daTuChoiLichNay)
            {
                TempData["Error"] =
                    "Người chăm sóc này đã từ chối lịch trước đó. "
                    + "Vui lòng chọn người khác.";


                return RedirectToAction(
                    nameof(Assign),
                    new
                    {
                        id = model.MaDatLich
                    });
            }
            // ========================================================
            // 4. KIỂM TRA TRẠNG THÁI HỒ SƠ
            // ========================================================

            bool hoSoHopLe =
                nguoiChamSoc.TrangThai == "Đang hoạt động"
                ||
                nguoiChamSoc.TrangThai == "Đã duyệt";

            if (!hoSoHopLe)
            {
                TempData["Error"] =
                    "Người chăm sóc chưa được duyệt hoặc không hoạt động.";

                return RedirectToAction(
                    nameof(Assign),
                    new
                    {
                        id = model.MaDatLich
                    });
            }


            // ========================================================
            // 5. KIỂM TRA TÀI KHOẢN CAREGIVER
            // ========================================================

            if (nguoiChamSoc.MaTaiKhoan.HasValue)
            {
                var taiKhoanCaregiver =
                    await _context.TaiKhoans
                        .AsNoTracking()
                        .FirstOrDefaultAsync(x =>
                            x.MaTaiKhoan
                                == nguoiChamSoc.MaTaiKhoan.Value);

                if (taiKhoanCaregiver == null)
                {
                    TempData["Error"] =
                        "Người chăm sóc chưa có tài khoản hợp lệ.";

                    return RedirectToAction(
                        nameof(Assign),
                        new
                        {
                            id = model.MaDatLich
                        });
                }


                if (!taiKhoanCaregiver.TrangThai)
                {
                    TempData["Error"] =
                        "Tài khoản của người chăm sóc đang bị khóa.";

                    return RedirectToAction(
                        nameof(Assign),
                        new
                        {
                            id = model.MaDatLich
                        });
                }
            }


            // ========================================================
            // 6. LẤY CẤU HÌNH GỢI Ý
            // ========================================================

            var cauHinh =
                await _context.CauHinhGoiYs
                    .AsNoTracking()
                    .Where(x =>
                        x.DangApDung)
                    .OrderByDescending(x =>
                        x.NgayCapNhat)
                    .FirstOrDefaultAsync();


            if (cauHinh != null)
            {
                // ====================================================
                // 6.1 KIỂM TRA KINH NGHIỆM TỐI THIỂU
                // ====================================================

                int kinhNghiem =
                    nguoiChamSoc.KinhNghiem ?? 0;

                


                // ====================================================
                // 6.2 TÍNH ĐÁNH GIÁ THỰC TẾ
                // ====================================================

                double? diemDanhGiaThucTe =
                    await (
                        from danhGia
                            in _context.DanhGias
                                .AsNoTracking()

                        join datLich
                            in _context.DatLichs
                                .AsNoTracking()

                            on danhGia.MaDatLich
                            equals datLich.MaDatLich

                        where
                            datLich.MaNguoiChamSoc
                                == nguoiChamSoc.MaNguoiChamSoc

                        select (double?)danhGia.SoSao
                    )
                    .AverageAsync();


                double diemDanhGia =
                    diemDanhGiaThucTe
                    ??
                    nguoiChamSoc.DanhGia
                    ??
                    0d;


                
            }


            // ========================================================
            // 7. KIỂM TRA TRÙNG LỊCH LẦN CUỐI
            //
            // Đây là bước quan trọng:
            // GET có thể đã mở từ vài phút trước.
            // Trong thời gian đó caregiver có thể vừa nhận lịch khác.
            // ========================================================

            bool biTrungLich =
                await _context.DatLichs
                    .AsNoTracking()
                    .AnyAsync(x =>
                        x.MaDatLich
                            != lich.MaDatLich

                        &&
                        x.MaNguoiChamSoc
                            == model.MaNguoiChamSoc.Value

                        &&
                        x.NgayChamSoc.HasValue

                        &&
                        x.GioBatDau.HasValue

                        &&
                        x.GioKetThuc.HasValue

                        &&
                        x.NgayChamSoc.Value
                            >= ngayChamSoc

                        &&
                        x.NgayChamSoc.Value
                            < ngayKeTiep

                        &&
                        x.TrangThai
                            != "Đã hủy"

                        &&
                        x.TrangThai
                            != "Đã hoàn thành"

                        &&
                        x.GioBatDau.Value
                            < gioKetThuc

                        &&
                        x.GioKetThuc.Value
                            > gioBatDau);


            if (biTrungLich)
            {
                TempData["Error"] =
                    "Người chăm sóc vừa phát sinh lịch trùng khung giờ này. "
                    + "Vui lòng chọn người khác.";

                return RedirectToAction(
                    nameof(Assign),
                    new
                    {
                        id = model.MaDatLich
                    });
            }


            // ========================================================
            // 8. LẤY THÔNG TIN KHÁCH HÀNG
            // ========================================================

            var khachHang =
                await _context.KhachHangs
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x =>
                        x.MaKhachHang
                            == lich.MaKhachHang);


            // ========================================================
            // 9. LẤY THÔNG TIN NGƯỜI ĐƯỢC CHĂM SÓC
            // ========================================================

            var benhNhan =
                await _context.BenhNhans
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x =>
                        x.MaBenhNhan
                            == lich.MaBenhNhan);


            // ========================================================
            // 10. LẤY DỊCH VỤ
            // ========================================================

            var dichVu =
                await _context.DichVus
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x =>
                        x.MaDichVu
                            == lich.MaDichVu);


            // ========================================================
            // 11. DÙNG TRANSACTION
            //
            // Phân công + tạo thông báo phải cùng thành công.
            // ========================================================

            await using var transaction =
                await _context.Database
                    .BeginTransactionAsync();


            try
            {
                // ====================================================
                // 12. GÁN NGƯỜI CHĂM SÓC
                // ====================================================

                lich.MaNguoiChamSoc =
                    nguoiChamSoc.MaNguoiChamSoc;


                await _context.SaveChangesAsync();


                // ====================================================
                // 13. THÔNG BÁO CHO KHÁCH HÀNG
                // ====================================================

                if (khachHang != null)
                {
                    var thongBaoKhachHang =
                        new ThongBao
                        {
                            MaTaiKhoan =
                                khachHang.MaTaiKhoan,

                            TieuDe =
                                "Lịch chăm sóc đã được phân công",

                            NoiDung =
                                $"Lịch #DL{lich.MaDatLich} cho "
                                +
                                $"{benhNhan?.HoTen ?? "người thân"} "
                                +
                                $"đã được phân công cho "
                                +
                                $"{nguoiChamSoc.HoTen ?? "người chăm sóc"}. "
                                +
                                $"Thời gian: "
                                +
                                $"{ngayChamSoc:dd/MM/yyyy} "
                                +
                                $"{gioBatDau:hh\\:mm} - "
                                +
                                $"{gioKetThuc:hh\\:mm}.",

                            DaDoc = false,

                            NgayGui =
                                DateTime.Now
                        };


                    _context.ThongBaos.Add(
                        thongBaoKhachHang);
                }


                // ====================================================
                // 14. THÔNG BÁO CHO NGƯỜI CHĂM SÓC
                // ====================================================

                if (nguoiChamSoc.MaTaiKhoan.HasValue)
                {
                    var thongBaoCaregiver =
                        new ThongBao
                        {
                            MaTaiKhoan =
                                nguoiChamSoc.MaTaiKhoan.Value,

                            TieuDe =
                                "Bạn có lịch chăm sóc mới",

                            NoiDung =
                                $"Bạn vừa được phân công lịch "
                                +
                                $"#DL{lich.MaDatLich} - "
                                +
                                $"{dichVu?.TenDichVu ?? "Dịch vụ chăm sóc"} "
                                +
                                $"cho "
                                +
                                $"{benhNhan?.HoTen ?? "người được chăm sóc"}. "
                                +
                                $"Thời gian: "
                                +
                                $"{ngayChamSoc:dd/MM/yyyy} "
                                +
                                $"{gioBatDau:hh\\:mm} - "
                                +
                                $"{gioKetThuc:hh\\:mm}.",

                            DaDoc = false,

                            NgayGui =
                                DateTime.Now
                        };


                    _context.ThongBaos.Add(
                        thongBaoCaregiver);
                }


                // ====================================================
                // 15. LƯU THÔNG BÁO
                // ====================================================

                await _context.SaveChangesAsync();


                // ====================================================
                // 16. COMMIT
                // ====================================================

                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();

                TempData["Error"] =
                    "Không thể hoàn tất phân công người chăm sóc. "
                    + "Vui lòng thử lại.";

                return RedirectToAction(
                    nameof(Assign),
                    new
                    {
                        id = model.MaDatLich
                    });
            }
            

            // ========================================================
            // 17. THÔNG BÁO THÀNH CÔNG
            // ========================================================

            TempData["Success"] =
                $"Đã phân công {nguoiChamSoc.HoTen} "
                +
                $"cho lịch #DL{lich.MaDatLich}.";


            return RedirectToAction(
                nameof(Index));
        }
        [HttpGet]
        public async Task<IActionResult> Index(
            string? tuKhoa,
            string? trangThai,
            string? khuVuc,
            DateTime? tuNgay,
            DateTime? denNgay)
        {
            tuKhoa = tuKhoa?.Trim() ?? string.Empty;
            trangThai = trangThai?.Trim() ?? string.Empty;
            khuVuc = khuVuc?.Trim() ?? string.Empty;

            
            var query =
                from lich in _context.DatLichs.AsNoTracking()

                join khachHang in _context.KhachHangs.AsNoTracking()
                    on lich.MaKhachHang
                    equals khachHang.MaKhachHang

                join benhNhan in _context.BenhNhans.AsNoTracking()
                    on lich.MaBenhNhan
                    equals benhNhan.MaBenhNhan

                join nguoiChamSocTam
                    in _context.NguoiChamSocs.AsNoTracking()
                    on lich.MaNguoiChamSoc
                    equals nguoiChamSocTam.MaNguoiChamSoc
                    into nhomNguoiChamSoc

                from nguoiChamSoc
                    in nhomNguoiChamSoc.DefaultIfEmpty()

                select new AdminBookingItemViewModel
                {
                    MaDatLich =
                        lich.MaDatLich,

                    TenKhachHang =
                        string.IsNullOrWhiteSpace(khachHang.HoTen)
                            ? "Khách hàng"
                            : khachHang.HoTen,

                    TenBenhNhan =
                        string.IsNullOrWhiteSpace(benhNhan.HoTen)
                            ? "Người được chăm sóc"
                            : benhNhan.HoTen,

                    TenNguoiChamSoc =
                        nguoiChamSoc == null
                            ? "Chưa phân công"
                            : nguoiChamSoc.HoTen,

                    KhuVuc =
                        string.IsNullOrWhiteSpace(benhNhan.DiaChi)
                            ? "Chưa cập nhật"
                            : benhNhan.DiaChi,
                    LyDoHuy = lich.LyDoHuy,

                    NguoiHuy = lich.NguoiHuy,

                    NgayHuy = lich.NgayHuy,
                    NgayChamSoc =
                        lich.NgayChamSoc ?? DateTime.Today,

                    GioBatDau =
                        lich.GioBatDau ?? TimeSpan.Zero,

                    GioKetThuc =
                        lich.GioKetThuc ?? TimeSpan.Zero,

                    TongTien =
                        lich.TongTien ?? 0m,

                    TrangThai =
                        lich.TrangThai ?? "Chưa xác định",

                    DaPhanCong =
                        lich.MaNguoiChamSoc.HasValue
                };

            if (!string.IsNullOrWhiteSpace(tuKhoa))
            {
                query = query.Where(x =>
                    x.TenKhachHang.Contains(tuKhoa)
                    ||
                    x.TenBenhNhan.Contains(tuKhoa)
                    ||
                    x.TenNguoiChamSoc.Contains(tuKhoa)
                    ||
                    x.MaDatLich.ToString().Contains(tuKhoa));
            }

            if (!string.IsNullOrWhiteSpace(trangThai))
            {
                query = query.Where(x =>
                    x.TrangThai == trangThai);
            }



            if (tuNgay.HasValue)
            {
                DateTime ngayBatDau =
                    tuNgay.Value.Date;

                query = query.Where(x =>
                    x.NgayChamSoc >= ngayBatDau);
            }

            if (denNgay.HasValue)
            {
                DateTime ngayKetThuc =
                    denNgay.Value.Date.AddDays(1);

                query = query.Where(x =>
                    x.NgayChamSoc < ngayKetThuc);
            }

            var danhSachLich =
                await query
                    .OrderBy(x =>
                        x.TrangThai == "Chờ xác nhận"
                            ? 0
                            : 1)
                    .ThenByDescending(x =>
                        x.NgayChamSoc)
                    .ThenBy(x =>
                        x.GioBatDau)
                    .ToListAsync();
            foreach (var item in danhSachLich)
            {
                item.KhuVuc =
                    LayKhuVucTuDiaChi(item.KhuVuc);
            }

            if (!string.IsNullOrWhiteSpace(khuVuc))
            {
                danhSachLich =
                    danhSachLich
                        .Where(x =>
                            string.Equals(
                                x.KhuVuc.Trim(),
                                khuVuc.Trim(),
                                StringComparison.OrdinalIgnoreCase))
                        .ToList();
            }
            var danhSachDiaChi =
    await _context.BenhNhans
        .AsNoTracking()
        .Where(x =>
            x.DiaChi != null &&
            x.DiaChi != "")
        .Select(x => x.DiaChi!)
        .ToListAsync();

            var danhSachKhuVuc =
                danhSachDiaChi
                    .Select(LayKhuVucTuDiaChi)
                    .Where(x =>
                        !string.IsNullOrWhiteSpace(x))
                    .Distinct()
                    .OrderBy(x => x)
                    .ToList();

            var model =
                new AdminBookingViewModel
                {
                    TongLich =
                        await _context.DatLichs
                            .AsNoTracking()
                            .CountAsync(),

                    ChoXacNhan =
                        await _context.DatLichs
                            .AsNoTracking()
                            .CountAsync(x =>
                                x.TrangThai ==
                                "Chờ xác nhận"),

                    DaXacNhan =
                        await _context.DatLichs
                            .AsNoTracking()
                            .CountAsync(x =>
                                x.TrangThai ==
                                "Đã xác nhận"),

                    DangThucHien =
                        await _context.DatLichs
                            .AsNoTracking()
                            .CountAsync(x =>
                                x.TrangThai ==
                                "Đang thực hiện"),

                    DaHoanThanh =
                        await _context.DatLichs
                            .AsNoTracking()
                            .CountAsync(x =>
                                x.TrangThai ==
                                "Đã hoàn thành"),

                    DaHuy =
                        await _context.DatLichs
                            .AsNoTracking()
                            .CountAsync(x =>
                                x.TrangThai ==
                                "Đã hủy"),

                    TuKhoa =
                        tuKhoa,

                    TrangThai =
                        trangThai,

                    KhuVuc =
                        khuVuc,

                    TuNgay =
                        tuNgay,

                    DenNgay =
                        denNgay,

                    DanhSachKhuVuc =
                        danhSachKhuVuc,

                    DanhSachLich =
                        danhSachLich
                };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(
            AdminUpdateBookingStatusViewModel model)
        {
            string[] trangThaiHopLe =
    BookingStatus.TatCa;

            if (!trangThaiHopLe.Contains(model.TrangThai))
            {
                TempData["Error"] =
                    "Trạng thái lịch đặt không hợp lệ.";

                return RedirectToAction(nameof(Index));
            }

            var lich =
                await _context.DatLichs
                    .FirstOrDefaultAsync(x =>
                        x.MaDatLich ==
                        model.MaDatLich);

            if (lich == null)
            {
                TempData["Error"] =
                    "Không tìm thấy lịch đặt.";

                return RedirectToAction(nameof(Index));
            }

            if (model.TrangThai == "Đã xác nhận" &&
                lich.MaNguoiChamSoc == null)
            {
                TempData["Error"] =
                    "Cần phân công người chăm sóc trước "
                    + "khi xác nhận lịch.";

                return RedirectToAction(nameof(Index));
            }

            bool chuyenTrangThaiHopLe =
                (
                    lich.TrangThai ==
            BookingStatus.ChoXacNhan
        &&
        model.TrangThai ==
            BookingStatus.DaXacNhan
    )
    ||
    (
        lich.TrangThai ==
            BookingStatus.DaXacNhan
        &&
        model.TrangThai ==
            BookingStatus.DangThucHien
    )
    ||
    (
        lich.TrangThai ==
            BookingStatus.DangThucHien
        &&
        model.TrangThai ==
            BookingStatus.DaHoanThanh
                );

            if (!chuyenTrangThaiHopLe)
            {
                TempData["Error"] =
                    $"Không thể chuyển lịch từ “{lich.TrangThai}” "
                    + $"sang “{model.TrangThai}”.";

                return RedirectToAction(nameof(Index));
            }

            lich.TrangThai =
                model.TrangThai;

            await _context.SaveChangesAsync();

            TempData["Success"] =
                $"Đã cập nhật lịch #DL{lich.MaDatLich} "
                + $"thành “{model.TrangThai}”.";

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancel(
            int maDatLich,
            string? lyDoHuy)
        {
            var lich =
                await _context.DatLichs
                    .FirstOrDefaultAsync(x =>
                        x.MaDatLich == maDatLich);

            if (lich == null)
            {
                TempData["Error"] =
                    "Không tìm thấy lịch đặt.";

                return RedirectToAction(nameof(Index));
            }

            lyDoHuy = lyDoHuy?.Trim();

            string[] danhSachLyDoHopLe =
            {
                "Không thể bố trí người chăm sóc phù hợp.",
                "Người chăm sóc được phân công không còn khả dụng.",
                "Thông tin lịch đặt chưa đầy đủ hoặc không hợp lệ.",
                "Lịch bị trùng thời gian với lịch khác.",
                "Dịch vụ hiện tạm ngưng cung cấp.",
                "Khách hàng yêu cầu hủy qua điện thoại."
            };

            if (string.IsNullOrWhiteSpace(lyDoHuy)
                || !danhSachLyDoHopLe.Contains(lyDoHuy))
            {
                TempData["Error"] =
                    "Lý do hủy lịch không hợp lệ.";

                return RedirectToAction(nameof(Index));
            }

            if (lich.TrangThai == "Đã hoàn thành")
            {
                TempData["Error"] =
                    "Không thể hủy lịch đã hoàn thành.";

                return RedirectToAction(nameof(Index));
            }

            if (lich.TrangThai == "Đã hủy")
            {
                TempData["Error"] =
                    "Lịch này đã được hủy trước đó.";

                return RedirectToAction(nameof(Index));
            }

            lich.TrangThai = "Đã hủy";
            lich.LyDoHuy = lyDoHuy;
            lich.NguoiHuy = User.Identity?.Name ?? "Admin";
            lich.NgayHuy = DateTime.Now;

            await _context.SaveChangesAsync();

            TempData["Success"] =
                $"Đã hủy lịch #DL{lich.MaDatLich}.";

            return RedirectToAction(nameof(Index));
        }
    }
}