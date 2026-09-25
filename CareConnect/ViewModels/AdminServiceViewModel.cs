using System.ComponentModel.DataAnnotations;

namespace CareConnect.ViewModels
{
    public class AdminServiceViewModel
    {
        public int TongDichVu { get; set; }

        public int DangHoatDong { get; set; }

        public int TamNgung { get; set; }

        public decimal GiaTrungBinh { get; set; }

        public string TuKhoa { get; set; } = string.Empty;

        public string TrangThai { get; set; } = string.Empty;

        public List<AdminServiceItemViewModel> DanhSachDichVu
        { get; set; } = new();
    }

    public class AdminServiceItemViewModel
    {
        public int MaDichVu { get; set; }

        public string MaDichVuCode { get; set; } = string.Empty;

        public string TenDichVu { get; set; } = string.Empty;

        public string MoTa { get; set; } = string.Empty;

        public decimal Gia { get; set; }

        public int ThoiLuong { get; set; }

        public bool TrangThai { get; set; }

        public DateTime NgayTao { get; set; }
    }

    public class AdminServiceFormViewModel
    {
        public int MaDichVu { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập tên dịch vụ.")]
        [StringLength(
            150,
            ErrorMessage = "Tên dịch vụ không được quá 150 ký tự.")]
        [Display(Name = "Tên dịch vụ")]
        public string TenDichVu { get; set; } = string.Empty;

        [StringLength(
            1000,
            ErrorMessage = "Mô tả không được quá 1000 ký tự.")]
        [Display(Name = "Mô tả")]
        public string? MoTa { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập giá dịch vụ.")]
        [Range(
            0,
            100000000,
            ErrorMessage = "Giá dịch vụ không hợp lệ.")]
        [Display(Name = "Giá dịch vụ")]
        public decimal Gia { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập thời lượng.")]
        [Range(
            1,
            10080,
            ErrorMessage = "Thời lượng phải lớn hơn 0 phút.")]
        [Display(Name = "Thời lượng")]
        public int ThoiLuong { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập mã dịch vụ.")]
        [StringLength(
            20,
            ErrorMessage = "Mã dịch vụ không được quá 20 ký tự.")]
        [Display(Name = "Mã dịch vụ")]
        public string MaDichVuCode { get; set; } = string.Empty;

        [Display(Name = "Đang hoạt động")]
        public bool TrangThai { get; set; } = true;
    }
}