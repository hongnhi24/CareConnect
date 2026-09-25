namespace CareConnect.ViewModels
{
    public class CaregiverIncomeViewModel
    {
        public decimal TongThuNhap { get; set; }

        public decimal ThuNhapThangNay { get; set; }

        public decimal DangChoThanhToan { get; set; }

        public int SoGiaoDichDaThanhToan { get; set; }

        public int SoGiaoDichChoThanhToan { get; set; }

        public string TuKhoa { get; set; } = string.Empty;

        public string TrangThai { get; set; } = string.Empty;

        public string LocNhanh { get; set; } = string.Empty;

        public DateTime? TuNgay { get; set; }

        public DateTime? DenNgay { get; set; }

        public List<CaregiverMonthlyIncomeViewModel> ThuNhapTheoThang
        {
            get;
            set;
        } = new();

        public List<CaregiverIncomeItemViewModel> DanhSachGiaoDich
        {
            get;
            set;
        } = new();
    }

    public class CaregiverMonthlyIncomeViewModel
    {
        public int Thang { get; set; }

        public int Nam { get; set; }

        public string NhanThang { get; set; } = string.Empty;

        public decimal SoTien { get; set; }

        public int PhanTram { get; set; }
    }

    public class CaregiverIncomeItemViewModel
    {
        public int MaThanhToan { get; set; }

        public string MaThanhToanCode { get; set; }
            = "Chưa cập nhật";

        public int MaDatLich { get; set; }

        public string TenKhachHang { get; set; }
            = "Chưa cập nhật";

        public string TenBenhNhan { get; set; }
            = "Chưa cập nhật";

        public string TenDichVu { get; set; }
            = "Chưa cập nhật";

        public DateTime NgayChamSoc { get; set; }

        public decimal SoTien { get; set; }

        public string PhuongThucThanhToan { get; set; }
            = "Chưa cập nhật";

        public string TrangThai { get; set; }
            = "Chưa thanh toán";

        public DateTime? NgayThanhToan { get; set; }

        public DateTime NgayTao { get; set; }

        public bool DaThanhToan =>
            TrangThai == "Đã thanh toán";
    }
}