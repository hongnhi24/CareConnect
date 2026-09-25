using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace CareConnect.ViewModels
{
    public class CustomerBookingCreateViewModel
    {
        [Required(
            ErrorMessage =
                "Vui lòng chọn người cần chăm sóc.")]
        [Display(Name = "Người cần chăm sóc")]
        public int? MaBenhNhan { get; set; }

        [Required(
            ErrorMessage =
                "Vui lòng chọn dịch vụ chăm sóc.")]
        [Display(Name = "Dịch vụ chăm sóc")]
        public int? MaDichVu { get; set; }

        [Required(
            ErrorMessage =
                "Vui lòng chọn ngày chăm sóc.")]
        [DataType(DataType.Date)]
        [Display(Name = "Ngày chăm sóc")]
        public DateTime? NgayChamSoc { get; set; }
        [Required(
    ErrorMessage =
        "Vui lòng chọn hình thức phân công.")]
        [Display(Name = "Hình thức chọn người chăm sóc")]
        public string HinhThucPhanCong { get; set; }
    = "Tự động";

        public int? MaNguoiChamSoc { get; set; }

        public List<CustomerCaregiverOptionViewModel>
            DanhSachNguoiChamSoc
        { get; set; }
            = new();
        [Required(
            ErrorMessage =
                "Vui lòng chọn giờ bắt đầu.")]
        [DataType(DataType.Time)]
        [Display(Name = "Giờ bắt đầu")]
        public TimeSpan? GioBatDau { get; set; }
        [Required(
    ErrorMessage =
        "Vui lòng chọn hình thức thanh toán.")]
        [Display(Name = "Hình thức thanh toán")]
        public string LoaiThanhToan { get; set; }
    = string.Empty;

        [Required(
            ErrorMessage =
                "Vui lòng chọn phương thức thanh toán.")]
        [Display(Name = "Phương thức thanh toán")]
        public string PhuongThucThanhToan { get; set; }
            = string.Empty;

        public decimal TyLeDatCoc { get; set; } = 30m;
        [Required(
            ErrorMessage =
                "Vui lòng nhập địa chỉ chăm sóc.")]
        [StringLength(
            500,
            ErrorMessage =
                "Địa chỉ không được vượt quá 500 ký tự.")]
        [Display(Name = "Địa chỉ chăm sóc")]
        public string DiaChiChamSoc { get; set; }
            = string.Empty;

        [StringLength(
            1000,
            ErrorMessage =
                "Ghi chú không được vượt quá 1000 ký tự.")]
        [Display(Name = "Ghi chú chăm sóc")]
        public string? GhiChu { get; set; }

        public string TenKhachHang { get; set; }
            = string.Empty;

        public string SoDienThoaiKhachHang { get; set; }
            = string.Empty;

        public int ThoiLuong { get; set; }

        public decimal DonGia { get; set; }

        public TimeSpan? GioKetThuc { get; set; }

        public decimal TongTien { get; set; }

        public List<SelectListItem>
            DanhSachBenhNhan
        { get; set; } = new();

        public List<CustomerServiceOptionViewModel>
            DanhSachDichVu
        { get; set; } = new();
    }

    public class CustomerServiceOptionViewModel
    {
        public int MaDichVu { get; set; }

        public string MaDichVuCode { get; set; }
            = string.Empty;

        public string TenDichVu { get; set; }
            = string.Empty;

        public string MoTa { get; set; }
            = string.Empty;

        public decimal Gia { get; set; }

        public int ThoiLuong { get; set; }
    }
    public class CustomerCaregiverOptionViewModel
    {
        public int MaNguoiChamSoc { get; set; }

        public string HoTen { get; set; }
            = string.Empty;

        public string ChuyenMon { get; set; }
            = string.Empty;

        public int KinhNghiem { get; set; }

        public double DanhGia { get; set; }

        public decimal GiaTheoGio { get; set; }

        public string KhuVucHoatDong { get; set; }
            = string.Empty;

        public double DiemPhuHop { get; set; }

        public string LyDoGoiY { get; set; }
            = string.Empty;
    }
}