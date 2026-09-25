using System.ComponentModel.DataAnnotations;

namespace CareConnect.ViewModels
{
    public class CaregiverProfileSetupViewModel
    {
        [Required(ErrorMessage = "Vui lòng nhập họ và tên.")]
        [StringLength(100)]
        [Display(Name = "Họ và tên")]
        public string HoTen { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập số điện thoại.")]
        [StringLength(15)]
        [Display(Name = "Số điện thoại")]
        public string SoDienThoai { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng chọn chuyên môn.")]
        [Display(Name = "Chuyên môn")]
        public List<string> ChuyenMon { get; set; } = new();

        [Range(
            0,
            50,
            ErrorMessage = "Số năm kinh nghiệm không hợp lệ.")]
        [Display(Name = "Số năm kinh nghiệm")]
        public int KinhNghiem { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập khu vực hoạt động.")]
        [StringLength(255)]
        [Display(Name = "Khu vực hoạt động")]
        public string KhuVucHoatDong { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập giá theo giờ.")]
        [Range(
            50000,
            1000000,
            ErrorMessage = "Giá theo giờ phải từ 50.000đ đến 1.000.000đ.")]
        [Display(Name = "Giá theo giờ")]
        public decimal GiaTheoGio { get; set; }

        [Required(ErrorMessage = "Vui lòng giới thiệu bản thân.")]
        [StringLength(
            500,
            MinimumLength = 20,
            ErrorMessage = "Giới thiệu phải từ 20 đến 500 ký tự.")]
        [Display(Name = "Giới thiệu bản thân")]
        public string GioiThieu { get; set; } = string.Empty;
    }
}