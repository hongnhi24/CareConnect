namespace CareConnect.Services
{
    public class CaregiverMatchRequest
    {
        public int MaBenhNhan { get; set; }

        public int MaDichVu { get; set; }

        public DateTime NgayChamSoc { get; set; }

        public TimeSpan GioBatDau { get; set; }

        public TimeSpan GioKetThuc { get; set; }

        public string? DiaChiChamSoc { get; set; }

        public string? GhiChu { get; set; }

        // Dùng khi Admin phân công lại một lịch đã tồn tại.
        // Không tính chính lịch đó là lịch bị trùng.
        public int? MaDatLichBoQua { get; set; }

        /*
         * Tự động:
         *   bắt buộc caregiver phải khai báo LichLamViec
         *   bao phủ đủ ca chăm sóc.
         *
         * Khách hàng tự chọn:
         *   không bắt buộc phải có LichLamViec.
         *   Caregiver vẫn phải đang hoạt động và
         *   không bị trùng với DatLich khác.
         *
         * Mặc định true để giữ an toàn cho các luồng
         * tự động và các chỗ gọi cũ.
         */
        public bool BatBuocCoLichKhaDung { get; set; } = true;
    }


    public class CaregiverMatchItem
    {
        public int MaNguoiChamSoc { get; set; }

        public string HoTen { get; set; }
            = "Người chăm sóc";

        public string ChuyenMon { get; set; }
            = "Chưa cập nhật";

        public string KhuVucHoatDong { get; set; }
            = "Chưa cập nhật";

        public int KinhNghiem { get; set; }

        public decimal GiaTheoGio { get; set; }

        public double DanhGia { get; set; }

        public bool CoDanhGiaThucTe { get; set; }

        public string Email { get; set; }
            = string.Empty;

        public string SoDienThoai { get; set; }
            = string.Empty;


        // ============================
        // KẾT QUẢ GỢI Ý
        // ============================

        public int ThuHang { get; set; }

        public bool LaGoiY { get; set; }

        public bool DatNguongGoiY { get; set; }

        public string GhiChuNguongGoiY { get; set; }
            = string.Empty;

        public double DiemPhuHop { get; set; }

        public double DiemKhuVuc { get; set; }

        public double DiemChuyenMon { get; set; }

        public double DiemKinhNghiem { get; set; }

        public double DiemDanhGia { get; set; }

        public double DiemMucGia { get; set; }

        public double DiemLichTrong { get; set; }

        public int SoLichTrongNgay { get; set; }

        public string LyDoGoiY { get; set; }
            = string.Empty;
    }


    public class CaregiverMatchingResponse
    {
        public bool DangApDungGoiY { get; set; }

        public string TenCauHinhGoiY { get; set; }
            = "Cấu hình gợi ý mặc định";

        public int SoLuongGoiY { get; set; }

        public int TrongSoKhuVuc { get; set; }

        public int TrongSoChuyenMon { get; set; }

        public int TrongSoKinhNghiem { get; set; }

        public int TrongSoDanhGia { get; set; }

        public int TrongSoMucGia { get; set; }

        public int TrongSoLichTrong { get; set; }

        public List<CaregiverMatchItem>
            DanhSach
        { get; set; } = new();
    }
}
