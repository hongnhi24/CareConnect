namespace CareConnect.ViewModels
{
    public class AdminCaregiverListViewModel
    {
        public int TongHoSo { get; set; }

        public int ChoDuyet { get; set; }

        public int DaDuyet { get; set; }

        public int TuChoi { get; set; }

        public string TuKhoa { get; set; } = string.Empty;

        public string TrangThai { get; set; } = string.Empty;

        public List<AdminCaregiverListItemViewModel> DanhSach
        { get; set; } = new();
    }

    public class AdminCaregiverListItemViewModel
    {
        public int MaNguoiChamSoc { get; set; }

        public string HoTen { get; set; } = "Chưa cập nhật";

        public string Email { get; set; } = "Chưa cập nhật";

        public string SoDienThoai { get; set; } = "Chưa cập nhật";

        public string ChuyenMon { get; set; } = "Chưa cập nhật";

        public string KhuVucHoatDong { get; set; } = "Chưa cập nhật";

        public int KinhNghiem { get; set; }

        public decimal GiaTheoGio { get; set; }

        public string TrangThai { get; set; } = "Chưa xác định";

        public DateTime NgayTao { get; set; }
    }

    public class AdminCaregiverReviewViewModel
    {
        public int MaNguoiChamSoc { get; set; }

        public int? MaTaiKhoan { get; set; }

        public string HoTen { get; set; } = "Chưa cập nhật";

        public string Email { get; set; } = "Chưa cập nhật";

        public string SoDienThoai { get; set; } = "Chưa cập nhật";

        public string BangCap { get; set; } = "Chưa cập nhật";

        public string ChuyenMon { get; set; } = "Chưa cập nhật";

        public int KinhNghiem { get; set; }

        public string KhuVucHoatDong { get; set; } = "Chưa cập nhật";

        public decimal GiaTheoGio { get; set; }

        public double DanhGia { get; set; }

        public string GioiThieu { get; set; } = "Chưa cập nhật";

        public string TrangThai { get; set; } = "Chưa xác định";

        public DateTime NgayTao { get; set; }

        public string TenDangNhap { get; set; } = "Chưa cập nhật";

        public bool TrangThaiTaiKhoan { get; set; }
    }

    public class RejectCaregiverViewModel
    {
        public int MaNguoiChamSoc { get; set; }

        public string LyDo { get; set; } = string.Empty;
    }
}