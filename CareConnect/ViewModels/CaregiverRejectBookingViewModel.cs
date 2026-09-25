using System.ComponentModel.DataAnnotations;

namespace CareConnect.ViewModels
{
    public class CaregiverRejectBookingViewModel
    {
        [Range(
            1,
            int.MaxValue,
            ErrorMessage = "Lịch đặt không hợp lệ.")]
        public int MaDatLich { get; set; }


        [Required(
            ErrorMessage = "Vui lòng chọn lý do từ chối lịch.")]
        [StringLength(
            100,
            ErrorMessage = "Lý do không được vượt quá 100 ký tự.")]
        public string LyDo { get; set; }
            = string.Empty;


        [StringLength(
            500,
            ErrorMessage = "Ghi chú không được vượt quá 500 ký tự.")]
        public string? GhiChu { get; set; }


        /*
         * Dùng để biết sau khi xử lý thì quay lại
         * Dashboard hay trang Lịch làm việc.
         */
        [StringLength(30)]
        public string? ReturnTo { get; set; }
    }
}