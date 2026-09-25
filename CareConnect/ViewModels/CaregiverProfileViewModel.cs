using System.ComponentModel.DataAnnotations;

namespace CareConnect.ViewModels
{
    public class CaregiverProfileViewModel
    {
        public int MaNguoiChamSoc { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập họ và tên.")]
        [StringLength(
            100,
            ErrorMessage = "Họ tên không được vượt quá 100 ký tự.")]
        [Display(Name = "Họ và tên")]
        public string HoTen { get; set; } = string.Empty;

        [Display(Name = "Giới tính")]
        public string? GioiTinh { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "Ngày sinh")]
        public DateTime? NgaySinh { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập số điện thoại.")]
        [StringLength(
            15,
            ErrorMessage = "Số điện thoại không hợp lệ.")]
        [Display(Name = "Số điện thoại")]
        public string SoDienThoai { get; set; } = string.Empty;

        [EmailAddress(ErrorMessage = "Email không đúng định dạng.")]
        [StringLength(100)]
        [Display(Name = "Email")]
        public string? Email { get; set; }

        [StringLength(255)]
        [Display(Name = "Địa chỉ")]
        public string? DiaChi { get; set; }

        [StringLength(255)]
        [Display(Name = "Bằng cấp")]
        public string? BangCap { get; set; }

        [Range(
            0,
            50,
            ErrorMessage = "Số năm kinh nghiệm không hợp lệ.")]
        [Display(Name = "Số năm kinh nghiệm")]
        public int KinhNghiem { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập chuyên môn.")]
        [StringLength(255)]
        [Display(Name = "Chuyên môn")]
        public string ChuyenMon { get; set; } = string.Empty;

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
        public decimal? GiaTheoGio { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập phần giới thiệu.")]
        [StringLength(
            500,
            MinimumLength = 20,
            ErrorMessage = "Giới thiệu phải từ 20 đến 500 ký tự.")]
        [Display(Name = "Giới thiệu bản thân")]
        public string GioiThieu { get; set; } = string.Empty;

        public double DanhGia { get; set; }

        public string TrangThai { get; set; } = string.Empty;

        public DateTime NgayTao { get; set; }

        public int SoBuoiHoanThanh { get; set; }

        public int SoDanhGia { get; set; }
    }
}
