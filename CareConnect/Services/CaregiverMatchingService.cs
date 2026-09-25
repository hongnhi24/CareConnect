using CareConnect.Data;
using CareConnect.Models;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using System.Text;
namespace CareConnect.Services
{
    public class CaregiverMatchingService
        : ICaregiverMatchingService
    {
        private readonly CareConnectDbContext _context;

        public CaregiverMatchingService(
            CareConnectDbContext context)
        {
            _context = context;
        }


        // =====================================================
        // HÀM CHÍNH
        // =====================================================

        public async Task<CaregiverMatchingResponse>
            TimDanhSachAsync(
                CaregiverMatchRequest request,
                CancellationToken cancellationToken =
                    default)
        {
            if (request.MaBenhNhan <= 0)
            {
                throw new ArgumentException(
                    "Mã người cần chăm sóc không hợp lệ.");
            }

            if (request.MaDichVu <= 0)
            {
                throw new ArgumentException(
                    "Mã dịch vụ không hợp lệ.");
            }

            if (request.GioKetThuc
                <= request.GioBatDau)
            {
                throw new ArgumentException(
                    "Giờ kết thúc phải lớn hơn "
                    + "giờ bắt đầu.");
            }


            DateTime ngayChamSoc =
                request.NgayChamSoc.Date;

            DateTime ngayKeTiep =
                ngayChamSoc.AddDays(1);


            // =================================================
            // 1. NGƯỜI CẦN CHĂM SÓC
            // =================================================

            var benhNhan =
                await _context.BenhNhans
                    .AsNoTracking()
                    .FirstOrDefaultAsync(
                        x =>
                            x.MaBenhNhan
                                == request.MaBenhNhan,
                        cancellationToken);

            if (benhNhan == null)
            {
                return new CaregiverMatchingResponse();
            }


            // =================================================
            // 2. DỊCH VỤ
            // =================================================

            var dichVu =
                await _context.DichVus
                    .AsNoTracking()
                    .FirstOrDefaultAsync(
                        x =>
                            x.MaDichVu
                                == request.MaDichVu
                            &&
                            x.TrangThai,
                        cancellationToken);

            if (dichVu == null)
            {
                return new CaregiverMatchingResponse();
            }


            // =================================================
            // 3. CẤU HÌNH
            // =================================================

            CauHinhGoiY cauHinh =
                await LayCauHinhAsync(
                    cancellationToken);


            var response =
                new CaregiverMatchingResponse
                {
                    DangApDungGoiY =
                        cauHinh.DangApDung,

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
                        cauHinh.TrongSoLichTrong
                };


            // =================================================
            // 4. CHỈ CAREGIVER CÓ TÀI KHOẢN HOẠT ĐỘNG
            // =================================================

            var taiKhoanHoatDong =
                await _context.TaiKhoans
                    .AsNoTracking()
                    .Where(x =>
                        x.TrangThai)
                    .Select(x =>
                        x.MaTaiKhoan)
                    .ToListAsync(
                        cancellationToken);


            var danhSachNguoiChamSoc =
                await _context.NguoiChamSocs
                    .AsNoTracking()
                    .ToListAsync(
                        cancellationToken);


            /*
             * Khác code Admin cũ một chút:
             *
             * Bây giờ caregiver phải có tài khoản
             * hoạt động vì sau khi được phân công
             * họ còn phải đăng nhập để xác nhận lịch.
             */
            danhSachNguoiChamSoc =
                danhSachNguoiChamSoc
                    .Where(x =>
                        x.MaTaiKhoan.HasValue
                        &&
                        taiKhoanHoatDong.Contains(
                            x.MaTaiKhoan.Value))
                    .ToList();


            if (cauHinh.ChiGoiYNguoiDaDuyet)
            {
                danhSachNguoiChamSoc =
                    danhSachNguoiChamSoc
                        .Where(x =>
                            x.TrangThai
                                == "Đang hoạt động"
                            ||
                            x.TrangThai
                                == "Đã duyệt")
                        .ToList();
            }
            else
            {
                danhSachNguoiChamSoc =
                    danhSachNguoiChamSoc
                        .Where(x =>
                            x.TrangThai
                                != "Từ chối")
                        .ToList();
            }


            // =================================================
            // 5. LOẠI CAREGIVER TRÙNG LỊCH
            // =================================================

            var queryTrungLich =
                _context.DatLichs
                    .AsNoTracking()
                    .Where(x =>
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
                            < request.GioKetThuc
                        &&
                        x.GioKetThuc.Value
                            > request.GioBatDau);


            if (request.MaDatLichBoQua.HasValue)
            {
                int maDatLichBoQua =
                    request.MaDatLichBoQua.Value;

                queryTrungLich =
                    queryTrungLich.Where(x =>
                        x.MaDatLich
                            != maDatLichBoQua);
            }


            var danhSachBiTrung =
                await queryTrungLich
                    .Select(x =>
                        x.MaNguoiChamSoc!.Value)
                    .Distinct()
                    .ToListAsync(
                        cancellationToken);


            danhSachNguoiChamSoc =
                danhSachNguoiChamSoc
                    .Where(x =>
                        !danhSachBiTrung.Contains(
                            x.MaNguoiChamSoc))
                    .ToList();


            // =================================================
            // 6. KIỂM TRA LỊCH KHẢ DỤNG
            // =================================================

            /*
             * Lỗi ở phiên bản trước:
             * TimDanhSachAsync luôn ép caregiver phải có
             * LichLamViec bao phủ toàn bộ ca. Vì API của màn
             * hình "Tôi muốn tự chọn" cũng gọi hàm này nên
             * caregiver cũ/chưa khai báo lịch đều bị loại hết.
             *
             * Quy tắc mới:
             *
             * - TỰ ĐỘNG:
             *   BatBuocCoLichKhaDung = true
             *   => phải có lịch khả dụng bao phủ đủ ca.
             *
             * - KHÁCH HÀNG TỰ CHỌN:
             *   BatBuocCoLichKhaDung = false
             *   => không ép phải có LichLamViec.
             *      Vẫn giữ điều kiện tài khoản hoạt động,
             *      hồ sơ đã duyệt và KHÔNG trùng DatLich.
             */
            if (request.BatBuocCoLichKhaDung)
            {
                var caregiverIdsConLai =
                    danhSachNguoiChamSoc
                        .Select(x =>
                            x.MaNguoiChamSoc)
                        .ToList();


                var lichKhaDungTrongNgay =
                    await _context.LichLamViecs
                        .AsNoTracking()
                        .Where(x =>
                            caregiverIdsConLai.Contains(
                                x.MaNguoiChamSoc)
                            && x.NgayLam >= ngayChamSoc
                            && x.NgayLam < ngayKeTiep)
                        .ToListAsync(
                            cancellationToken);


                var caregiverCoDuKhungGio =
                    lichKhaDungTrongNgay
                        .Where(LaKhungKhaDung)
                        .GroupBy(x =>
                            x.MaNguoiChamSoc)
                        .Where(g =>
                            BaoPhuKhungYeuCau(
                                g.Select(x =>
                                    new KhoangThoiGian(
                                        x.GioBatDau,
                                        x.GioKetThuc)),
                                request.GioBatDau,
                                request.GioKetThuc))
                        .Select(g => g.Key)
                        .ToHashSet();


                danhSachNguoiChamSoc =
                    danhSachNguoiChamSoc
                        .Where(x =>
                            caregiverCoDuKhungGio.Contains(
                                x.MaNguoiChamSoc))
                        .ToList();
            }


            // =================================================
            // 7. ĐÁNH GIÁ THỰC TẾ
            // =================================================

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
                        x.DiemTrungBinh,
                    cancellationToken);


            // =================================================
            // 8. SỐ LỊCH TRONG NGÀY
            // =================================================

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
                            x.SoLich,
                        cancellationToken);


            // =================================================
            // 9. NẾU ADMIN TẮT THUẬT TOÁN
            // =================================================

            if (!cauHinh.DangApDung)
            {
                var danhSachThuong =
                    new List<CaregiverMatchItem>();

                foreach (
                    var ncs
                    in danhSachNguoiChamSoc)
                {
                    double danhGia =
                        danhGiaThucTe.TryGetValue(
                            ncs.MaNguoiChamSoc,
                            out double diemThucTe)

                            ? diemThucTe

                            : ncs.DanhGia ?? 0d;


                    danhSachThuong.Add(
                        new CaregiverMatchItem
                        {
                            MaNguoiChamSoc =
                                ncs.MaNguoiChamSoc,

                            HoTen =
                                ncs.HoTen
                                ?? "Người chăm sóc",

                            ChuyenMon =
                                ncs.ChuyenMon
                                ?? "Chưa cập nhật",

                            KhuVucHoatDong =
                                ncs.KhuVucHoatDong
                                ?? "Chưa cập nhật",

                            KinhNghiem =
                                ncs.KinhNghiem ?? 0,

                            GiaTheoGio =
                                ncs.GiaTheoGio ?? 0,

                            DanhGia =
                                Math.Round(
                                    danhGia,
                                    1),

                            Email =
                                ncs.Email
                                ?? string.Empty,

                            SoDienThoai =
                                ncs.SoDienThoai
                                ?? string.Empty,

                            DatNguongGoiY =
                                true,

                            LyDoGoiY =
                                "Thuật toán gợi ý "
                                + "đang tạm dừng."
                        });
                }


                response.DanhSach =
                    danhSachThuong
                        .OrderByDescending(x =>
                            x.DanhGia)
                        .ThenByDescending(x =>
                            x.KinhNghiem)
                        .ThenBy(x =>
                            x.HoTen)
                        .ToList();


                return response;
            }


            // =================================================
            // 10. THÔNG TIN YÊU CẦU
            // =================================================

            string diaChiChamSoc =
                !string.IsNullOrWhiteSpace(
                    request.DiaChiChamSoc)

                    ? request.DiaChiChamSoc!

                    : benhNhan.DiaChi
                        ?? string.Empty;


            string yeuCauChamSoc =
                string.Join(
                    " ",
                    new[]
                    {
                        dichVu.TenDichVu,
                        dichVu.MoTa,
                        benhNhan.TinhTrangSucKhoe,
                        benhNhan.TienSuBenh,
                        benhNhan.GhiChu,
                        request.GhiChu
                    }
                    .Where(x =>
                        !string.IsNullOrWhiteSpace(x)));


            // =================================================
            // 11. GIÁ MỤC TIÊU / GIỜ
            // =================================================

            decimal giaMucTieuTheoGio = 0;

            if (dichVu.ThoiLuong > 0
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


            // =================================================
            // 12. TỔNG TRỌNG SỐ
            // =================================================

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


            if (tongTrongSo <= 0)
            {
                tongTrongSo = 100;
            }


            // =================================================
            // 13. CHẤM ĐIỂM
            // =================================================

            var ketQua =
                new List<CaregiverMatchItem>();


            foreach (
                var nguoiChamSoc
                in danhSachNguoiChamSoc)
            {
                bool coDanhGiaThucTe =
                    danhGiaThucTe.TryGetValue(
                        nguoiChamSoc.MaNguoiChamSoc,
                        out double diemDanhGiaThucTe);


                double diemSao =
                    coDanhGiaThucTe
                        ? diemDanhGiaThucTe
                        : nguoiChamSoc.DanhGia
                            ?? 0;


                bool datKinhNghiem =
                    (nguoiChamSoc.KinhNghiem ?? 0)
                    >=
                    cauHinh.KinhNghiemToiThieu;


                bool datDanhGia =
                    diemSao
                    >=
                    (double)
                    cauHinh.DiemDanhGiaToiThieu;


                bool datNguong =
                    datKinhNghiem
                    &&
                    datDanhGia;


                var ghiChuNguong =
                    new List<string>();


                if (!datKinhNghiem)
                {
                    ghiChuNguong.Add(
                        $"Kinh nghiệm dưới "
                        + $"{cauHinh.KinhNghiemToiThieu} năm");
                }


                if (!datDanhGia)
                {
                    ghiChuNguong.Add(
                        $"Đánh giá dưới "
                        + $"{cauHinh.DiemDanhGiaToiThieu:0.0} sao");
                }


                int soLich =
                    soLichTrongNgay.TryGetValue(
                        nguoiChamSoc.MaNguoiChamSoc,
                        out int soLichTimThay)

                        ? soLichTimThay

                        : 0;


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


                double diemKhuVuc =
                    TinhDiemKhuVuc(
                        diaChiChamSoc,
                        nguoiChamSoc.KhuVucHoatDong,
                        cauHinh.UuTienCungKhuVuc);


                double diemChuyenMon =
                    TinhDiemChuyenMon(
                        yeuCauChamSoc,
                        thongTinChuyenMon);


                double diemKinhNghiem =
                    TinhDiemKinhNghiem(
                        nguoiChamSoc.KinhNghiem
                        ?? 0);


                double diemDanhGia =
                    TinhDiemDanhGia(
                        diemSao);


                double diemMucGia =
                    TinhDiemMucGia(
                        nguoiChamSoc.GiaTheoGio
                        ?? 0,

                        giaMucTieuTheoGio);


                double diemLichTrong =
                    TinhDiemLichTrong(
                        soLich);


                double diemPhuHop =
                    (
                        diemKhuVuc
                        *
                        cauHinh.TrongSoKhuVuc

                        +

                        diemChuyenMon
                        *
                        cauHinh.TrongSoChuyenMon

                        +

                        diemKinhNghiem
                        *
                        cauHinh.TrongSoKinhNghiem

                        +

                        diemDanhGia
                        *
                        cauHinh.TrongSoDanhGia

                        +

                        diemMucGia
                        *
                        cauHinh.TrongSoMucGia

                        +

                        diemLichTrong
                        *
                        cauHinh.TrongSoLichTrong
                    )
                    /
                    tongTrongSo;


                diemPhuHop =
                    Math.Round(
                        GioiHanDiem(
                            diemPhuHop),
                        1);


                ketQua.Add(
                    new CaregiverMatchItem
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
                            nguoiChamSoc.KhuVucHoatDong
                            ?? "Chưa cập nhật",

                        KinhNghiem =
                            nguoiChamSoc.KinhNghiem
                            ?? 0,

                        GiaTheoGio =
                            nguoiChamSoc.GiaTheoGio
                            ?? 0,

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
                            nguoiChamSoc.SoDienThoai
                            ?? string.Empty,

                        DatNguongGoiY =
                            datNguong,

                        GhiChuNguongGoiY =
                            ghiChuNguong.Count == 0
                                ? "Đạt điều kiện gợi ý"
                                : string.Join(
                                    " • ",
                                    ghiChuNguong),

                        DiemPhuHop =
                            diemPhuHop,

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


            // =================================================
            // 14. XẾP HẠNG
            // =================================================

            ketQua =
                ketQua
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


            int soLuongGoiY =
                Math.Max(
                    1,
                    cauHinh.SoLuongGoiY);


            int thuHang = 1;


            foreach (var item in ketQua)
            {
                if (item.DatNguongGoiY
                    &&
                    thuHang <= soLuongGoiY)
                {
                    item.ThuHang =
                        thuHang;

                    item.LaGoiY =
                        true;

                    thuHang++;
                }
                else
                {
                    item.ThuHang =
                        0;

                    item.LaGoiY =
                        false;
                }
            }


            response.DanhSach =
                ketQua;


            return response;
        }


        // =====================================================
        // TỰ ĐỘNG LẤY NGƯỜI TỐT NHẤT
        // =====================================================

        public async Task<CaregiverMatchItem?>
            TimNguoiTotNhatAsync(
                CaregiverMatchRequest request,
                CancellationToken cancellationToken =
                    default)
        {
            var response =
                await TimDanhSachAsync(
                    request,
                    cancellationToken);


            /*
             * Nếu Admin tắt thuật toán,
             * không cho hệ thống tự động phân công.
             */
            if (!response.DangApDungGoiY)
            {
                return null;
            }


            return response.DanhSach
                .FirstOrDefault(x =>
                    x.DatNguongGoiY);
        }


        // =====================================================
        // KIỂM TRA CAREGIVER KHÁCH TỰ CHỌN
        // =====================================================

        public async Task<CaregiverMatchItem?>
            KiemTraLuaChonAsync(
                int maNguoiChamSoc,
                CaregiverMatchRequest request,
                CancellationToken cancellationToken =
                    default)
        {
            var response =
                await TimDanhSachAsync(
                    request,
                    cancellationToken);


            /*
             * Khách hàng đã chọn thủ công thì chỉ cần
             * caregiver vẫn còn nằm trong danh sách đủ
             * điều kiện cơ bản do TimDanhSachAsync trả về.
             *
             * Không bắt buộc DatNguongGoiY ở đây.
             * Ngưỡng gợi ý chỉ áp dụng khi hệ thống
             * tự động chọn caregiver tốt nhất.
             */
            return response.DanhSach
                .FirstOrDefault(x =>
                    x.MaNguoiChamSoc
                        == maNguoiChamSoc);
        }


        // =====================================================
        // CẤU HÌNH
        // =====================================================

        private async Task<CauHinhGoiY>
            LayCauHinhAsync(
                CancellationToken cancellationToken)
        {
            var cauHinh =
                await _context.CauHinhGoiYs
                    .AsNoTracking()
                    .OrderByDescending(x =>
                        x.DangApDung)
                    .ThenByDescending(x =>
                        x.NgayCapNhat)
                    .FirstOrDefaultAsync(
                        cancellationToken);


            return cauHinh
                ??
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


        // =====================================================
        // CÁC HÀM TÍNH ĐIỂM
        // =====================================================

        private static double GioiHanDiem(
            double diem)
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


        private static string LayKhuVucTuDiaChi(
            string? diaChi)
        {
            if (string.IsNullOrWhiteSpace(
                    diaChi))
            {
                return "Chưa cập nhật";
            }


            string[] cacPhan =
                diaChi.Split(
                    ',',
                    StringSplitOptions
                        .RemoveEmptyEntries);


            foreach (string phan in cacPhan)
            {
                string giaTri =
                    phan.Trim();


                if (giaTri.StartsWith(
                        "Quận ",
                        StringComparison
                            .OrdinalIgnoreCase))
                {
                    return giaTri;
                }


                if (giaTri.StartsWith(
                        "Huyện ",
                        StringComparison
                            .OrdinalIgnoreCase))
                {
                    return giaTri;
                }


                if (giaTri.Contains(
                        "Thủ Đức",
                        StringComparison
                            .OrdinalIgnoreCase))
                {
                    return "TP. Thủ Đức";
                }
            }


            return "Khu vực khác";
        }


        private static string ChuanHoaVanBan(
            string? vanBan)
        {
            if (string.IsNullOrWhiteSpace(
                    vanBan))
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
                    CharUnicodeInfo
                        .GetUnicodeCategory(
                            kyTu);


                if (category
                    ==
                    UnicodeCategory.NonSpacingMark)
                {
                    continue;
                }


                if (char.IsLetterOrDigit(
                        kyTu))
                {
                    builder.Append(
                        kyTu);
                }
                else
                {
                    builder.Append(
                        ' ');
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


        private static HashSet<string>
            TachTuKhoa(
                string? vanBan)
        {
            string daChuanHoa =
                ChuanHoaVanBan(
                    vanBan);


            if (string.IsNullOrWhiteSpace(
                    daChuanHoa))
            {
                return new HashSet<string>();
            }


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


        private static bool LaKhungKhaDung(
            LichLamViec lich)
        {
            if (lich.GioKetThuc
                <= lich.GioBatDau)
            {
                return false;
            }

            string trangThai =
                lich.TrangThai?.Trim()
                ?? string.Empty;

            string[] trangThaiKhongKhaDung =
            {
                "Không khả dụng",
                "Nghỉ",
                "Tạm nghỉ",
                "Đã hủy"
            };

            return !trangThaiKhongKhaDung.Contains(
                trangThai,
                StringComparer.OrdinalIgnoreCase);
        }


        private static bool BaoPhuKhungYeuCau(
            IEnumerable<KhoangThoiGian> cacKhung,
            TimeSpan gioBatDau,
            TimeSpan gioKetThuc)
        {
            if (gioKetThuc <= gioBatDau)
            {
                return false;
            }

            var danhSach =
                cacKhung
                    .Where(x => x.End > x.Start)
                    .OrderBy(x => x.Start)
                    .ThenBy(x => x.End)
                    .ToList();

            if (danhSach.Count == 0)
            {
                return false;
            }

            TimeSpan daBaoPhuDen =
                gioBatDau;

            foreach (var khung in danhSach)
            {
                if (khung.End <= daBaoPhuDen)
                {
                    continue;
                }

                if (khung.Start > daBaoPhuDen)
                {
                    return false;
                }

                daBaoPhuDen =
                    khung.End;

                if (daBaoPhuDen >= gioKetThuc)
                {
                    return true;
                }
            }

            return false;
        }


        private readonly record struct KhoangThoiGian(
            TimeSpan Start,
            TimeSpan End);


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


            if (diaChi.Contains(
                    khuVuc))
            {
                return 100;
            }


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


            foreach (
                string item
                in cacKhuVuc)
            {
                string khuVucCon =
                    ChuanHoaVanBan(
                        item);


                if (!string.IsNullOrWhiteSpace(
                        khuVucCon)
                    &&
                    diaChi.Contains(
                        khuVucCon))
                {
                    return 100;
                }
            }


            string khuVucLich =
                ChuanHoaVanBan(
                    LayKhuVucTuDiaChi(
                        diaChiChamSoc));


            if (!string.IsNullOrWhiteSpace(
                    khuVucLich)
                &&
                khuVuc.Contains(
                    khuVucLich))
            {
                return 100;
            }


            return 35;
        }


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


            if (tuKhoaYeuCau.Count == 0
                ||
                tuKhoaCaregiver.Count == 0)
            {
                return 50;
            }


            int soTuTrung =
                tuKhoaYeuCau.Count(x =>
                    tuKhoaCaregiver.Contains(x));


            if (soTuTrung == 0)
            {
                return 35;
            }


            double tyLe =
                (double)soTuTrung
                /
                tuKhoaYeuCau.Count;


            return GioiHanDiem(
                Math.Max(
                    55,
                    tyLe * 100));
        }


        private static double TinhDiemKinhNghiem(
            int kinhNghiem)
        {
            if (kinhNghiem <= 0)
            {
                return 0;
            }


            return GioiHanDiem(
                kinhNghiem
                /
                10.0
                *
                100.0);
        }


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


            return GioiHanDiem(
                100
                -
                (double)(
                    tyLeVuotGia
                    *
                    100m));
        }


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


            return lyDo.Count == 0

                ? "phù hợp theo tổng hợp nhiều tiêu chí"

                : string.Join(
                    " • ",
                    lyDo.Take(3));
        }
    }
}