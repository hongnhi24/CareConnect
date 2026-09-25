namespace CareConnect.ViewModels
{
    public class CaregiverReviewViewModel
    {
        public double DiemTrungBinh { get; set; }

        public int TongDanhGia { get; set; }

        public int TyLeNamSao { get; set; }

        public int TongBuoiHoanThanh { get; set; }

        public int NamSao { get; set; }

        public int BonSao { get; set; }

        public int BaSao { get; set; }

        public int HaiSao { get; set; }

        public int MotSao { get; set; }

        public string TuKhoa { get; set; }
            = string.Empty;

        public int? SoSao { get; set; }

        public DateTime? TuNgay { get; set; }

        public DateTime? DenNgay { get; set; }

        public List<CaregiverReviewItemViewModel> DanhSachDanhGia
        {
            get;
            set;
        } = new();

        public List<CaregiverHistoryItemViewModel> LichSuChamSoc
        {
            get;
            set;
        } = new();
    }

    public class CaregiverReviewItemViewModel
    {
        public int MaDanhGia { get; set; }

        public int MaDatLich { get; set; }

        public string TenKhachHang { get; set; }
            = "Khách hàng";

        public string TenBenhNhan { get; set; }
            = "Chưa cập nhật";

        public string TenDichVu { get; set; }
            = "Chưa cập nhật";

        public int SoSao { get; set; }

        public string NoiDung { get; set; }
            = string.Empty;

        public DateTime NgayDanhGia { get; set; }

        public DateTime NgayChamSoc { get; set; }
    }

    public class CaregiverHistoryItemViewModel
    {
        public int MaDatLich { get; set; }

        public string TenBenhNhan { get; set; }
            = "Chưa cập nhật";

        public string TenKhachHang { get; set; }
            = "Chưa cập nhật";

        public string TenDichVu { get; set; }
            = "Chưa cập nhật";

        public DateTime NgayChamSoc { get; set; }

        public TimeSpan GioBatDau { get; set; }

        public TimeSpan GioKetThuc { get; set; }

        public string DiaChiChamSoc { get; set; }
            = "Chưa cập nhật";

        public decimal TongTien { get; set; }

        public int? SoSao { get; set; }

        public string TrangThai { get; set; }
            = "Đã hoàn thành";
    }
}