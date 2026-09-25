using System.ComponentModel.DataAnnotations;

namespace CareConnect.ViewModels
{
    public class CustomerProfileViewModel
    {
        public int MaKhachHang { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập họ và tên.")]
        public string HoTen { get; set; } = string.Empty;

        public string? GioiTinh { get; set; }

        [DataType(DataType.Date)]
        public DateTime? NgaySinh { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập số điện thoại.")]
        public string SoDienThoai { get; set; } = string.Empty;

        [EmailAddress(ErrorMessage = "Email không hợp lệ.")]
        public string? Email { get; set; }

        public string? DiaChi { get; set; }

        public string? CCCD { get; set; }

        public int SoNguoiThan { get; set; }

        public int SoLichChamSoc { get; set; }
    }
}