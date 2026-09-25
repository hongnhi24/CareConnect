namespace CareConnect.ViewModels
{
    public class CaregiverScheduleViewModel
    {
        public int TongLich { get; set; }
        public string LocNhanh { get; set; }
    = string.Empty;
        public int ChoXacNhan { get; set; }

        public int SapToi { get; set; }

        public int DaHoanThanh { get; set; }

        public string TuKhoa { get; set; }
            = string.Empty;

        public string TrangThai { get; set; }
            = string.Empty;

        public DateTime? TuNgay { get; set; }

        public DateTime? DenNgay { get; set; }

        public List<CaregiverScheduleItemViewModel> DanhSachLich
        {
            get;
            set;
        } = new();
    }

    public class CaregiverScheduleItemViewModel
    {
        public int MaDatLich { get; set; }

        public string TenKhachHang { get; set; }
            = "Chưa cập nhật";

        public string SoDienThoaiKhachHang { get; set; }
            = "Chưa cập nhật";

        public string TenBenhNhan { get; set; }
            = "Chưa cập nhật";

        public string TinhTrangSucKhoe { get; set; }
            = "Chưa cập nhật";

        public string TenDichVu { get; set; }
            = "Chưa cập nhật";

        public DateTime NgayChamSoc { get; set; }

        public TimeSpan GioBatDau { get; set; }

        public TimeSpan GioKetThuc { get; set; }

        public string DiaChiChamSoc { get; set; }
            = "Chưa cập nhật";

        public decimal TongTien { get; set; }

        public string TrangThai { get; set; }
            = "Chưa xác định";

        public string GhiChu { get; set; }
            = string.Empty;

        public bool CoTheXacNhan =>
            TrangThai == "Chờ xác nhận";

        public bool CoTheBatDau =>
            TrangThai == "Đã xác nhận";

        public bool CoTheHoanThanh =>
            TrangThai == "Đang thực hiện";
    }

    public class CaregiverScheduleStatusUpdateViewModel
    {
        public int MaDatLich { get; set; }

        public string TrangThai { get; set; }
            = string.Empty;
    }
}