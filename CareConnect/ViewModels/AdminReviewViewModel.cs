namespace CareConnect.ViewModels
{
    public class AdminReviewViewModel
    {
        public int TongDanhGia { get; set; }

        public double DiemTrungBinh { get; set; }

        public int DanhGiaTot { get; set; }

        public int DanhGiaCanChuY { get; set; }

        public string TuKhoa { get; set; } = string.Empty;

        public int? SoSao { get; set; }

        public DateTime? TuNgay { get; set; }

        public DateTime? DenNgay { get; set; }

        public List<AdminReviewStarItemViewModel> PhanBoSoSao
        { get; set; } = new();

        public List<AdminReviewItemViewModel> DanhSachDanhGia
        { get; set; } = new();
    }

    public class AdminReviewStarItemViewModel
    {
        public int SoSao { get; set; }

        public int SoLuong { get; set; }

        public double TyLe { get; set; }
    }

    public class AdminReviewItemViewModel
    {
        public int MaDanhGia { get; set; }

        public int MaDatLich { get; set; }

        public int SoSao { get; set; }

        public string NoiDung { get; set; } = string.Empty;

        public DateTime NgayDanhGia { get; set; }

        public string TenKhachHang { get; set; }
            = "Chưa cập nhật";

        public string TenNguoiChamSoc { get; set; }
            = "Chưa phân công";

        public string TenBenhNhan { get; set; }
            = "Chưa cập nhật";

        public string TenDichVu { get; set; }
            = "Chưa cập nhật";

        public DateTime NgayChamSoc { get; set; }

        public string TrangThaiLich { get; set; }
            = "Chưa xác định";
    }
}