namespace CareConnect.ViewModels
{
    public class AdminUserManagementViewModel
    {
        public int TongNguoiDung { get; set; }

        public int TongKhachHang { get; set; }

        public int TongNguoiChamSoc { get; set; }

        public int SoNguoiChamSocChoDuyet { get; set; }

        public int SoTaiKhoanBiKhoa { get; set; }

        public string TuKhoa { get; set; } = string.Empty;

        public string VaiTro { get; set; } = string.Empty;

        public string TrangThai { get; set; } = string.Empty;

        public List<AdminUserItemViewModel> DanhSachNguoiDung
        { get; set; } = new();
    }

    public class AdminUserItemViewModel
    {
        public int MaTaiKhoan { get; set; }

        public int? MaNguoiChamSoc { get; set; }

        public string HoTen { get; set; } = "Chưa cập nhật";

        public string Email { get; set; } = string.Empty;

        public string TenDangNhap { get; set; } = string.Empty;

        public string TenVaiTro { get; set; } = "Chưa xác định";

        public bool TaiKhoanHoatDong { get; set; }

        public string TrangThaiHoSo { get; set; } = string.Empty;

        public DateTime NgayTao { get; set; }

        public string ChuyenMon { get; set; } = string.Empty;

        public string KhuVucHoatDong { get; set; } = string.Empty;

        public int KinhNghiem { get; set; }

        public string TrangThaiHienThi
        {
            get
            {
                if (TenVaiTro == "Người chăm sóc" &&
                    TrangThaiHoSo == "Chờ duyệt")
                {
                    return "Chờ duyệt";
                }

                if (TenVaiTro == "Người chăm sóc" &&
                    TrangThaiHoSo == "Từ chối")
                {
                    return "Đã từ chối";
                }

                return TaiKhoanHoatDong
                    ? "Hoạt động"
                    : "Tạm khóa";
            }
        }
    }
}