namespace CareConnect.ViewModels
{
    public class CustomerBookingViewModel
    {
        public int TongLich { get; set; }

        public int ChoXacNhan { get; set; }

        public int DaXacNhan { get; set; }

        public int DangThucHien { get; set; }

        public int DaHoanThanh { get; set; }

        public int DaHuy { get; set; }

        public string TrangThai { get; set; }
            = string.Empty;

        public List<CustomerBookingItemViewModel>
            DanhSachLich
        { get; set; } = new();
    }

    public class CustomerBookingItemViewModel
    {
        public int MaDatLich { get; set; }

        public string TenBenhNhan { get; set; }
            = "Người được chăm sóc";

        public string TenDichVu { get; set; }
            = "Dịch vụ chăm sóc";

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

        public string? LyDoHuy { get; set; }

        public string? NguoiHuy { get; set; }

        public DateTime? NgayHuy { get; set; }
    }

    public class CustomerBookingDetailsViewModel
    {
        public int MaDatLich { get; set; }

        public string TenKhachHang { get; set; }
            = "Khách hàng";

        public string TenBenhNhan { get; set; }
            = "Người được chăm sóc";

        public string TenDichVu { get; set; }
            = "Dịch vụ chăm sóc";

        public string MoTaDichVu { get; set; }
            = string.Empty;

        public string TenNguoiChamSoc { get; set; }
            = "Chưa phân công";

        public string SoDienThoaiNguoiChamSoc { get; set; }
            = string.Empty;

        public string ChuyenMonNguoiChamSoc { get; set; }
            = string.Empty;

        public string KhuVuc { get; set; }
            = "Chưa cập nhật";

        public DateTime NgayChamSoc { get; set; }

        public TimeSpan GioBatDau { get; set; }

        public TimeSpan GioKetThuc { get; set; }

        public decimal TongTien { get; set; }

        public string TrangThai { get; set; }
            = "Chưa xác định";

        public string? GhiChu { get; set; }

        public string? LyDoHuy { get; set; }

        public string? NguoiHuy { get; set; }

        public DateTime? NgayHuy { get; set; }
    }
}