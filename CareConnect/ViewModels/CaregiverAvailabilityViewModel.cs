using System.ComponentModel.DataAnnotations;

namespace CareConnect.ViewModels
{
    public class CaregiverAvailabilityViewModel
    {
        public DateTime DauTuan { get; set; }

        public DateTime CuoiTuan { get; set; }

        public DateTime HomNay { get; set; }

        public int TongKhungKhaDung { get; set; }

        public double TongGioKhaDung { get; set; }

        public List<CaregiverAvailabilityDayViewModel>
            DanhSachNgay
        { get; set; } = new();
    }

    public class CaregiverAvailabilityDayViewModel
    {
        public DateTime Ngay { get; set; }

        public string Thu { get; set; } = string.Empty;

        public bool LaHomNay { get; set; }

        public List<CaregiverAvailabilityItemViewModel>
            DanhSachKhungGio
        { get; set; } = new();
    }

    public class CaregiverAvailabilityItemViewModel
    {
        public int MaLich { get; set; }

        public DateTime NgayLam { get; set; }

        public TimeSpan GioBatDau { get; set; }

        public TimeSpan GioKetThuc { get; set; }

        public string TrangThai { get; set; }
            = "Khả dụng";

        public bool CoLichDat { get; set; }

        public int SoLichDat { get; set; }

        public double SoGio =>
            Math.Round(
                Math.Max(
                    0,
                    (GioKetThuc - GioBatDau)
                        .TotalHours),
                1);
    }

    public class CaregiverAvailabilityCreateViewModel
    {
        [Required(ErrorMessage = "Vui lòng chọn ngày làm việc.")]
        [DataType(DataType.Date)]
        public DateTime NgayLam { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn giờ bắt đầu.")]
        [DataType(DataType.Time)]
        public TimeSpan GioBatDau { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn giờ kết thúc.")]
        [DataType(DataType.Time)]
        public TimeSpan GioKetThuc { get; set; }
    }

    public class CaregiverAvailabilityUpdateViewModel
        : CaregiverAvailabilityCreateViewModel
    {
        [Required]
        public int MaLich { get; set; }
    }
}
