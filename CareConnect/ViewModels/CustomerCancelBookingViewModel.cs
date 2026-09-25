using System.ComponentModel.DataAnnotations;

namespace CareConnect.ViewModels
{
    public class CustomerCancelBookingViewModel
    {
        [Range(
            1,
            int.MaxValue,
            ErrorMessage = "Lịch đặt không hợp lệ.")]
        public int MaDatLich { get; set; }


        public string TenDichVu { get; set; }
            = string.Empty;


        public string TenBenhNhan { get; set; }
            = string.Empty;


        public DateTime NgayChamSoc { get; set; }


        public TimeSpan GioBatDau { get; set; }


        public TimeSpan GioKetThuc { get; set; }


        public string TrangThai { get; set; }
            = string.Empty;


        public decimal TongTien { get; set; }

        public decimal SoTienDaThanhToan { get; set; }
        public decimal TyLeHoanDuKien { get; set; }


        public decimal SoTienHoanDuKien { get; set; }


        public bool HuyGiuaChung { get; set; }


        public string ChinhSachApDung { get; set; }
            = string.Empty;


        [Required(
            ErrorMessage = "Vui lòng chọn lý do hủy lịch.")]
        [StringLength(
            100,
            ErrorMessage = "Lý do không được vượt quá 100 ký tự.")]
        public string LyDo { get; set; }
            = string.Empty;


        [StringLength(
            500,
            ErrorMessage = "Ghi chú không được vượt quá 500 ký tự.")]
        public string? GhiChu { get; set; }
    }
}