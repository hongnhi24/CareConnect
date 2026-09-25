using System.ComponentModel.DataAnnotations;

namespace CareConnect.ViewModels
{
    public class CustomerMedicationReminderFormViewModel
    {
        [Required(ErrorMessage = "Vui lòng chọn người dùng thuốc.")]
        public int? MaBenhNhan { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập tên thuốc.")]
        public string TenThuoc { get; set; } = string.Empty;

        public string? LieuDung { get; set; }

        public string? CachDung { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn giờ uống.")]
        public TimeSpan? ThoiGianUong { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn ngày bắt đầu.")]
        public DateTime? NgayBatDau { get; set; }

        public DateTime? NgayKetThuc { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn tần suất.")]
        public string TanSuat { get; set; } = "Hằng ngày";

        public string? GhiChu { get; set; }
    }


    // Dùng để hiển thị từng lịch thuốc
    public class CustomerMedicationReminderItemViewModel
    {
        public int MaLichNhac { get; set; }

        public string TenBenhNhan { get; set; } = string.Empty;

        public string TenThuoc { get; set; } = string.Empty;

        public string? LieuDung { get; set; }

        public string? CachDung { get; set; }

        public TimeSpan ThoiGianUong { get; set; }

        public DateTime NgayBatDau { get; set; }

        public DateTime? NgayKetThuc { get; set; }

        public string TanSuat { get; set; } = string.Empty;

        public string? GhiChu { get; set; }

        public bool TrangThai { get; set; }

        public DateTime? LanNhacGanNhat { get; set; }
    }


    // Dùng cho dropdown chọn người thân
    public class CustomerMedicationRelativeOptionViewModel
    {
        public int MaBenhNhan { get; set; }

        public string HoTen { get; set; } = string.Empty;

        public string QuanHe { get; set; } = string.Empty;
    }


    // Model tổng của trang nhắc thuốc
    public class CustomerMedicationReminderPageViewModel
    {
        public CustomerMedicationReminderFormViewModel Form
        {
            get;
            set;
        } = new();

        public List<CustomerMedicationRelativeOptionViewModel> NguoiThan
        {
            get;
            set;
        } = new();

        public List<CustomerMedicationReminderItemViewModel> LichNhac
        {
            get;
            set;
        } = new();
    }
}