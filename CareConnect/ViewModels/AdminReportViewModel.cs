namespace CareConnect.ViewModels
{
    public class AdminReportViewModel
    {
        public int TongDanhGia { get; set; }

        public double DiemDanhGiaTrungBinh { get; set; }

        public int DanhGiaTichCuc { get; set; }

        public int DanhGiaCanChuY { get; set; }

        public List<AdminReportReviewStarViewModel> PhanBoDanhGia
        { get; set; } = new();

        public List<AdminTopCaregiverReviewViewModel> TopNguoiChamSoc
        { get; set; } = new();
        public int NamDangChon { get; set; }

        public string PhamVi { get; set; } = "all";

        public List<int> DanhSachNam { get; set; } = new();

        // Tổng quan hệ thống
        public int TongKhachHang { get; set; }

        public int TongNguoiChamSoc { get; set; }

        public int TongLichDat { get; set; }

        public int HoSoChoDuyet { get; set; }

        // Chỉ số tháng hiện tại
        public int LichTrongThang { get; set; }

        public decimal GiaTriHoanThanhTrongThang { get; set; }

        // Chỉ số theo phạm vi
        public int TongLuotDatTrongKy { get; set; }

        public decimal TongDoanhThuTrongKy { get; set; }

        public int NguoiDungMoiTrongKy { get; set; }

        public double TangTruongTrungBinh { get; set; }

        public double TyLeHoanThanh { get; set; }

        public double TyLeHuy { get; set; }

        // Dữ liệu báo cáo
        public List<AdminMonthlyReportItemViewModel> BaoCaoTheoThang
        { get; set; } = new();

        public List<AdminBookingStatusReportItemViewModel> PhanBoTrangThai
        { get; set; } = new();

        public List<AdminAreaReportItemViewModel> BaoCaoKhuVuc
        { get; set; } = new();

        public List<AdminServiceReportItemViewModel> BaoCaoDichVu
        { get; set; } = new();

        public List<AdminRecentBookingReportItemViewModel> LichGanDay
        { get; set; } = new();
    }
    public class AdminReportReviewStarViewModel
    {
        public int SoSao { get; set; }

        public int SoLuong { get; set; }

        public double TyLe { get; set; }
    }

    public class AdminTopCaregiverReviewViewModel
    {
        public int MaNguoiChamSoc { get; set; }

        public string HoTen { get; set; } = string.Empty;

        public int SoLuotDanhGia { get; set; }

        public double DiemTrungBinh { get; set; }
    }
    public class AdminMonthlyReportItemViewModel
    {
        public int Thang { get; set; }

        public int Nam { get; set; }

        public string NhanThang { get; set; } = string.Empty;

        public int SoLich { get; set; }

        public int SoLichHoanThanh { get; set; }

        public decimal GiaTriHoanThanh { get; set; }

        public int NguoiDungMoi { get; set; }

        public decimal TrungBinhMoiLuot { get; set; }

        public double TangTruong { get; set; }
    }

    public class AdminBookingStatusReportItemViewModel
    {
        public string TrangThai { get; set; } = string.Empty;

        public int SoLuong { get; set; }

        public double TyLe { get; set; }
    }

    public class AdminAreaReportItemViewModel
    {
        public string KhuVuc { get; set; } = string.Empty;

        public int SoLich { get; set; }

        public int SoLichHoanThanh { get; set; }

        public decimal TongGiaTri { get; set; }
    }

    public class AdminServiceReportItemViewModel
    {
        public string TenDichVu { get; set; } = string.Empty;

        public int SoLuotDat { get; set; }

        public decimal TongDoanhThu { get; set; }

        public double TyLe { get; set; }
    }

    public class AdminRecentBookingReportItemViewModel
    {
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
    }
}