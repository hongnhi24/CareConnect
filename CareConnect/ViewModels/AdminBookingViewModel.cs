namespace CareConnect.ViewModels
{
    public class AdminBookingViewModel
    {

        public int TongLich { get; set; }

        public int ChoXacNhan { get; set; }

        public int DaXacNhan { get; set; }

        public int DangThucHien { get; set; }

        public int DaHoanThanh { get; set; }

        public int DaHuy { get; set; }

        public string TuKhoa { get; set; } = string.Empty;

        public string TrangThai { get; set; } = string.Empty;

        public string KhuVuc { get; set; } = string.Empty;

        public DateTime? TuNgay { get; set; }

        public DateTime? DenNgay { get; set; }

        public List<string> DanhSachKhuVuc { get; set; } = new();

        public List<AdminBookingItemViewModel> DanhSachLich
        { get; set; } = new();
    }

    public class AdminBookingItemViewModel
    {
        public string? LyDoHuy { get; set; }

        public string? NguoiHuy { get; set; }

        public DateTime? NgayHuy { get; set; }
        public int MaDatLich { get; set; }

        public string TenKhachHang { get; set; }
            = "Chưa cập nhật";

        public string TenBenhNhan { get; set; }
            = "Chưa cập nhật";

        public string TenNguoiChamSoc { get; set; }
            = "Chưa phân công";

        public string KhuVuc { get; set; }
            = "Chưa cập nhật";

        public DateTime NgayChamSoc { get; set; }

        public TimeSpan GioBatDau { get; set; }

        public TimeSpan GioKetThuc { get; set; }

        public decimal TongTien { get; set; }

        public string TrangThai { get; set; }
            = "Chưa xác định";

        public bool DaPhanCong { get; set; }
    }

    public class AdminUpdateBookingStatusViewModel
    {
        public int MaDatLich { get; set; }

        public string TrangThai { get; set; } = string.Empty;
    }
}