using System.ComponentModel.DataAnnotations;

namespace CareConnect.ViewModels
{
    public class CustomerAppointmentReminderFormViewModel
    {
        [Required(ErrorMessage = "Vui lòng chọn người cần tái khám.")]
        [Display(Name = "Người cần tái khám")]
        public int? MaBenhNhan { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn ngày tái khám.")]
        [Display(Name = "Ngày tái khám")]
        [DataType(DataType.Date)]
        public DateTime? NgayTaiKham { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn giờ tái khám.")]
        [Display(Name = "Giờ tái khám")]
        [DataType(DataType.Time)]
        public TimeSpan? GioTaiKham { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập nội dung tái khám.")]
        [StringLength(300, ErrorMessage = "Nội dung tối đa 300 ký tự.")]
        [Display(Name = "Nội dung")]
        public string NoiDung { get; set; } = string.Empty;

        [StringLength(255, ErrorMessage = "Địa điểm tối đa 255 ký tự.")]
        [Display(Name = "Địa điểm")]
        public string? DiaDiem { get; set; }

        [StringLength(500, ErrorMessage = "Ghi chú tối đa 500 ký tự.")]
        [Display(Name = "Ghi chú")]
        public string? GhiChu { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn thời điểm nhắc trước.")]
        [Range(5, 10080, ErrorMessage = "Thời gian nhắc trước không hợp lệ.")]
        [Display(Name = "Nhắc trước")]
        public int SoPhutNhacTruoc { get; set; } = 1440;
    }

    public class CustomerAppointmentReminderItemViewModel
    {
        public int MaLichNhacTaiKham { get; set; }
        public string TenBenhNhan { get; set; } = string.Empty;
        public DateTime NgayTaiKham { get; set; }
        public TimeSpan GioTaiKham { get; set; }
        public string NoiDung { get; set; } = string.Empty;
        public string? DiaDiem { get; set; }
        public string? GhiChu { get; set; }
        public int SoPhutNhacTruoc { get; set; }
        public bool TrangThai { get; set; }
        public DateTime? LanNhacGanNhat { get; set; }
    }

    public class CustomerAppointmentRelativeOptionViewModel
    {
        public int MaBenhNhan { get; set; }
        public string HoTen { get; set; } = string.Empty;
        public string QuanHe { get; set; } = string.Empty;
    }

    public class CustomerAppointmentReminderPageViewModel
    {
        public CustomerAppointmentReminderFormViewModel Form { get; set; } = new();
        public List<CustomerAppointmentRelativeOptionViewModel> NguoiThan { get; set; } = new();
        public List<CustomerAppointmentReminderItemViewModel> LichNhac { get; set; } = new();
    }
}
