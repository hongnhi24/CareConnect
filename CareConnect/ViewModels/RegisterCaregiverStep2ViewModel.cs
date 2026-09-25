using System.ComponentModel.DataAnnotations;

namespace CareConnect.ViewModels
{
    public class RegisterCaregiverStep2ViewModel
    {
        [Required(ErrorMessage = "Vui lòng chọn ít nhất một chuyên môn.")]
        public List<string> ChuyenMon { get; set; } = new();

        [Required(ErrorMessage = "Vui lòng chọn số năm kinh nghiệm.")]
        [Range(0, 50, ErrorMessage = "Số năm kinh nghiệm không hợp lệ.")]
        [Display(Name = "Số năm kinh nghiệm")]
        public int? SoNamKinhNghiem { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập bằng cấp.")]
        [StringLength(150)]
        [Display(Name = "Bằng cấp")]
        public string BangCap { get; set; } = string.Empty;

        [StringLength(150)]
        [Display(Name = "Tên chứng chỉ")]
        public string? TenChungChi { get; set; }

        [StringLength(150)]
        [Display(Name = "Đơn vị cấp")]
        public string? DonViCap { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "Ngày cấp")]
        public DateTime? NgayCap { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "Ngày hết hạn")]
        public DateTime? NgayHetHan { get; set; }
    }
}