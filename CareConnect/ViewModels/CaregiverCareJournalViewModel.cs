using System.ComponentModel.DataAnnotations;

namespace CareConnect.ViewModels
{
    public class CaregiverCareJournalViewModel
    {
        public int TongLichCoTheGhi { get; set; }

        public int DaGhiNhatKy { get; set; }

        public int ChuaGhiNhatKy { get; set; }

        public int CapNhatHomNay { get; set; }

        public string TuKhoa { get; set; }
            = string.Empty;

        public string TrangThaiNhatKy { get; set; }
            = string.Empty;

        public DateTime? TuNgay { get; set; }

        public DateTime? DenNgay { get; set; }

        public List<CaregiverCareJournalItemViewModel> DanhSach
        {
            get;
            set;
        } = new();
    }

    public class CaregiverCareJournalItemViewModel
    {
        public int MaDatLich { get; set; }

        public int? MaNhatKy { get; set; }

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

        public string TrangThaiLich { get; set; }
            = "Chưa xác định";

        public string? TinhTrangSucKhoe { get; set; }

        public string? HuyetAp { get; set; }

        public int? NhipTim { get; set; }

        public decimal? NhietDo { get; set; }

        public decimal? CanNang { get; set; }

        public string? TinhTrangAnUong { get; set; }

        public string? ThuocDaUong { get; set; }

        public string? GhiChu { get; set; }

        public DateTime? NgayCapNhat { get; set; }

        public bool DaCoNhatKy =>
            MaNhatKy.HasValue;
    }

    public class CaregiverCareJournalSaveViewModel
    {
        public int MaDatLich { get; set; }

        public int? MaNhatKy { get; set; }

        [StringLength(
            1500,
            ErrorMessage =
                "Tình trạng sức khỏe không được vượt quá 1.500 ký tự.")]
        [Display(Name = "Tình trạng sức khỏe")]
        public string? TinhTrangSucKhoe { get; set; }

        [StringLength(
            30,
            ErrorMessage =
                "Huyết áp không được vượt quá 30 ký tự.")]
        [Display(Name = "Huyết áp")]
        public string? HuyetAp { get; set; }

        [Range(
            20,
            250,
            ErrorMessage =
                "Nhịp tim phải nằm trong khoảng 20–250 lần/phút.")]
        [Display(Name = "Nhịp tim")]
        public int? NhipTim { get; set; }

        [Range(
            30,
            45,
            ErrorMessage =
                "Nhiệt độ phải nằm trong khoảng 30–45°C.")]
        [Display(Name = "Nhiệt độ")]
        public decimal? NhietDo { get; set; }

        [Range(
            1,
            500,
            ErrorMessage =
                "Cân nặng phải nằm trong khoảng 1–500 kg.")]
        [Display(Name = "Cân nặng")]
        public decimal? CanNang { get; set; }

        [StringLength(
            500,
            ErrorMessage =
                "Tình trạng ăn uống không được vượt quá 500 ký tự.")]
        [Display(Name = "Tình trạng ăn uống")]
        public string? TinhTrangAnUong { get; set; }

        [StringLength(
            500,
            ErrorMessage =
                "Thuốc đã uống không được vượt quá 500 ký tự.")]
        [Display(Name = "Thuốc đã uống")]
        public string? ThuocDaUong { get; set; }

        [StringLength(
            1500,
            ErrorMessage =
                "Ghi chú không được vượt quá 1.500 ký tự.")]
        [Display(Name = "Ghi chú")]
        public string? GhiChu { get; set; }
    }
}