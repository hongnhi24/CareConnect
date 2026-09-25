namespace CareConnect.ViewModels
{
    public class CaregiverDashboardViewModel
    {
        public string HoTen { get; set; }
            = "Người chăm sóc";

        public string ChuyenMon { get; set; }
            = "Chăm sóc người cao tuổi";

        public string KhuVucHoatDong { get; set; }
            = "Chưa cập nhật";

        public string TrangThaiHoSo { get; set; }
            = "Chưa cập nhật";

        public int KinhNghiem { get; set; }

        public double DanhGia { get; set; }

        public decimal GiaTheoGio { get; set; }

        public int SoBuoiHomNay { get; set; }

        public int SoYeuCauMoi { get; set; }

        public int SoBuoiHoanThanhThangNay { get; set; }

        public decimal ThuNhapThangNay { get; set; }

        public int SoThongBaoChuaDoc { get; set; }

        
        public List<CaregiverScheduleItemViewModel> LichSapToi
        {
            get;
            set;
        } = new();

        public List<CaregiverRequestItemViewModel> YeuCauChoXacNhan
        {
            get;
            set;
        } = new();

        public List<CaregiverNotificationItemViewModel> ThongBaoGanDay
        {
            get;
            set;
        } = new();
    }

    public class CaregiverRequestItemViewModel
    {
        public int MaDatLich { get; set; }

        public string TenBenhNhan { get; set; }
            = "Chưa cập nhật";

        public string TenKhachHang { get; set; }
            = "Chưa cập nhật";

        public DateTime NgayChamSoc { get; set; }

        public TimeSpan GioBatDau { get; set; }

        public TimeSpan GioKetThuc { get; set; }

        public string DiaChiChamSoc { get; set; }
            = "Chưa cập nhật";

        public decimal TongTien { get; set; }
    }

    public class CaregiverNotificationItemViewModel
    {
        public string TieuDe { get; set; }
            = "Thông báo";

        public string NoiDung { get; set; }
            = string.Empty;

        public DateTime NgayGui { get; set; }

        public bool DaDoc { get; set; }
    }
}