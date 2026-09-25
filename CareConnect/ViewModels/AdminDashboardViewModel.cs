namespace CareConnect.ViewModels
{
    public class AdminDashboardViewModel
    {
        public string TenQuanTriVien { get; set; } = "Quản trị viên";

        public int TongTaiKhoan { get; set; }

        public int TongKhachHang { get; set; }

        public int TongNguoiChamSoc { get; set; }

        public int HoSoChoDuyet { get; set; }

        public int LichChoXacNhan { get; set; }

        public int TongDatLichThang { get; set; }

        public int TongDatLichHomNay { get; set; }

        public decimal DoanhThuThang { get; set; }

        // Tỷ lệ lấp đầy thật: thời gian đã được đặt / thời gian caregiver mở lịch.
        public double TyLeLapDay { get; set; }

        public bool CoDuLieuLichLamViec { get; set; }

        public DateTime DauTuan { get; set; }

        public DateTime CuoiTuan { get; set; }

        public int TongDatLichTuan { get; set; }

        public int LichHoanThanhTuan { get; set; }

        public int LichDaHuyTuan { get; set; }

        public double TongGioMoLichTuan { get; set; }

        public double TongGioDaDatTuan { get; set; }

        public List<AdminDailyStatisticViewModel> ThongKeTheoNgayTrongTuan
        { get; set; } = new();

        public List<AdminMonthlyStatisticViewModel> ThongKeTheoThang
        { get; set; } = new();

        public List<AdminRecentBookingViewModel> LichDatGanDay
        { get; set; } = new();

        public List<AdminPendingCaregiverViewModel> HoSoChoDuyetGanDay
        { get; set; } = new();

        public List<AdminBookingStatusViewModel> ThongKeTrangThai
        { get; set; } = new();
    }

    public class AdminDailyStatisticViewModel
    {
        public DateTime Ngay { get; set; }

        public string NhanNgay { get; set; } = string.Empty;

        public int SoDatLich { get; set; }

        public int SoHoanThanh { get; set; }

        public int SoDaHuy { get; set; }

        public double GioMoLich { get; set; }

        public double GioDaDat { get; set; }

        public double? TyLeLapDay { get; set; }
    }

    public class AdminMonthlyStatisticViewModel
    {
        public int Thang { get; set; }

        public decimal DoanhThu { get; set; }

        public int SoDatLich { get; set; }
    }

    public class AdminRecentBookingViewModel
    {
        public int MaDatLich { get; set; }

        public string TenKhachHang { get; set; } =
            "Chưa cập nhật";

        public string TenBenhNhan { get; set; } =
            "Chưa cập nhật";

        public string TenNguoiChamSoc { get; set; } =
            "Chưa phân công";

        public DateTime NgayChamSoc { get; set; }

        public TimeSpan GioBatDau { get; set; }

        public TimeSpan GioKetThuc { get; set; }

        public decimal TongTien { get; set; }

        public string TrangThai { get; set; } =
            "Chưa xác định";
    }

    public class AdminPendingCaregiverViewModel
    {
        public int MaNguoiChamSoc { get; set; }

        public string HoTen { get; set; } =
            "Chưa cập nhật";

        public string ChuyenMon { get; set; } =
            "Chưa cập nhật";

        public string KhuVucHoatDong { get; set; } =
            "Chưa cập nhật";

        public int KinhNghiem { get; set; }

        public DateTime NgayTao { get; set; }
    }

    public class AdminBookingStatusViewModel
    {
        public string TrangThai { get; set; } =
            "Khác";

        public int SoLuong { get; set; }
    }
}
