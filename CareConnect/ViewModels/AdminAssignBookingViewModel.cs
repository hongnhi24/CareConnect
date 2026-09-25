using System.ComponentModel.DataAnnotations;

namespace CareConnect.ViewModels
{
    public class AdminAssignBookingViewModel
    {
        public int MaDatLich { get; set; }

        public string TenKhachHang { get; set; }
            = "Khách hàng";

        public string TenBenhNhan { get; set; }
            = "Người được chăm sóc";

        public string TenDichVu { get; set; }
            = "Dịch vụ chăm sóc";

        public string KhuVuc { get; set; }
            = "Chưa cập nhật";

        public DateTime NgayChamSoc { get; set; }

        public TimeSpan GioBatDau { get; set; }

        public TimeSpan GioKetThuc { get; set; }

        public decimal TongTien { get; set; }


        [Required(
            ErrorMessage =
                "Vui lòng chọn người chăm sóc.")]
        public int? MaNguoiChamSoc { get; set; }


        // =====================================================
        // THÔNG TIN CẤU HÌNH THUẬT TOÁN GỢI Ý
        // =====================================================

        // Có đang sử dụng thuật toán gợi ý hay không
        public bool DangApDungGoiY { get; set; }


        // Tên cấu hình đang lấy từ bảng CauHinhGoiY
        public string TenCauHinhGoiY { get; set; }
            = "Cấu hình gợi ý mặc định";


        // Số lượng caregiver sẽ được đánh dấu là Top gợi ý
        public int SoLuongGoiY { get; set; } = 5;


        // Các trọng số
        public int TrongSoKhuVuc { get; set; }

        public int TrongSoChuyenMon { get; set; }

        public int TrongSoKinhNghiem { get; set; }

        public int TrongSoDanhGia { get; set; }

        public int TrongSoMucGia { get; set; }

        public int TrongSoLichTrong { get; set; }


        public List<AdminAssignableCaregiverViewModel>
            DanhSachNguoiChamSoc
        { get; set; }
            = new();
    }


    public class AdminAssignableCaregiverViewModel
    {
        // =====================================================
        // THÔNG TIN NGƯỜI CHĂM SÓC
        // =====================================================

        public int MaNguoiChamSoc { get; set; }
        public bool DatNguongGoiY { get; set; }

        public string GhiChuNguongGoiY { get; set; }
            = string.Empty;

        public string HoTen { get; set; }
            = "Người chăm sóc";


        public string ChuyenMon { get; set; }
            = "Chưa cập nhật";


        public string KhuVucHoatDong { get; set; }
            = "Chưa cập nhật";


        public int KinhNghiem { get; set; }


        public decimal GiaTheoGio { get; set; }


        public double DanhGia { get; set; }


        // Cho biết điểm đánh giá được tính từ bảng DanhGia
        // hay chỉ đang dùng điểm mặc định trong hồ sơ
        public bool CoDanhGiaThucTe { get; set; }


        public string Email { get; set; }
            = string.Empty;


        public string SoDienThoai { get; set; }
            = string.Empty;


        // =====================================================
        // KẾT QUẢ THUẬT TOÁN GỢI Ý
        // =====================================================

   
        public int ThuHang { get; set; }


        // true nếu nằm trong Top N được hệ thống đề xuất
        public bool LaGoiY { get; set; }


        // Điểm cuối cùng từ 0 - 100
        public double DiemPhuHop { get; set; }


        // =====================================================
        // ĐIỂM 6 TIÊU CHÍ
        // Mỗi tiêu chí đều từ 0 - 100
        // =====================================================

        public double DiemKhuVuc { get; set; }

        public double DiemChuyenMon { get; set; }

        public double DiemKinhNghiem { get; set; }

        public double DiemDanhGia { get; set; }

        public double DiemMucGia { get; set; }

        public double DiemLichTrong { get; set; }


        // Số lịch caregiver đã có trong ngày đó.
        // Dùng để tính tải công việc.
        public int SoLichTrongNgay { get; set; }


        // Ví dụ:
        // "Cùng khu vực • chuyên môn phù hợp • đánh giá tốt"
        public string LyDoGoiY { get; set; }
            = string.Empty;
    }
}