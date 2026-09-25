using System.ComponentModel.DataAnnotations;

namespace CareConnect.ViewModels
{
    public class RegisterCaregiverStep3ViewModel
    {
        [Required(ErrorMessage = "Vui lòng nhập khu vực hoạt động.")]
        [StringLength(
            250,
            ErrorMessage = "Khu vực hoạt động không được vượt quá 250 ký tự.")]
        [Display(Name = "Khu vực hoạt động")]
        public string KhuVucHoatDong { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập giá mỗi giờ.")]
        [Range(
            50000,
            1000000,
            ErrorMessage = "Giá mỗi giờ phải từ 50.000 đến 1.000.000 đồng.")]
        [Display(Name = "Giá mỗi giờ")]
        public decimal? GiaMoiGio { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập phần giới thiệu bản thân.")]
        [StringLength(
            500,
            MinimumLength = 20,
            ErrorMessage = "Giới thiệu phải từ 20 đến 500 ký tự.")]
        [Display(Name = "Giới thiệu bản thân")]
        public string GioiThieu { get; set; } = string.Empty;
    }
}