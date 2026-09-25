namespace CareConnect.ViewModels
{
    public class CustomerDashboardViewModel
    {
        public string HoTenKhachHang { get; set; } = string.Empty;

        public int SoBuoiSapToi { get; set; }

        public int SoHoSoDangQuanLy { get; set; }

        public int SoBuoiHoanThanhThangNay { get; set; }

        public int SoThongBaoMoi { get; set; }

        public List<UpcomingBookingViewModel> LichSapToi { get; set; }
            = new();

        public List<CustomerNotificationViewModel> ThongBaoGanDay { get; set; }
            = new();
    }

    public class UpcomingBookingViewModel
    {
        public int MaDatLich { get; set; }

        public string TenNguoiChamSoc { get; set; } = string.Empty;

        public string TenBenhNhan { get; set; } = string.Empty;

        public DateTime? NgayChamSoc { get; set; }

        public TimeSpan? GioBatDau { get; set; }

        public TimeSpan? GioKetThuc { get; set; }

        public decimal? TongTien { get; set; }

        public string TrangThai { get; set; } = string.Empty;
    }

    public class CustomerNotificationViewModel
    {
        public int MaThongBao { get; set; }
        public string TieuDe { get; set; } = string.Empty;

        public string NoiDung { get; set; } = string.Empty;

        public DateTime NgayGui { get; set; }

        public bool DaDoc { get; set; }
    }
}