using System.ComponentModel.DataAnnotations;

namespace CareConnect.ViewModels
{
    public class CustomerPaymentListViewModel
    {
        public int TongKhoanThanhToan { get; set; }

        public int ChuaThanhToan { get; set; }

        public int DaDatCoc { get; set; }

        public int DaThanhToan { get; set; }

        public int DaHoanTien { get; set; }

        // Tổng số tiền khách thực tế đã trả,
        // bao gồm cả tiền cọc.
        public decimal TongDaThanhToan { get; set; }

        public string TrangThai { get; set; }
            = string.Empty;

        public List<CustomerPaymentItemViewModel>
            DanhSach
        { get; set; } = new();
    }

    public class CustomerPaymentItemViewModel
    {
        public int MaDatLich { get; set; }

        public int? MaThanhToan { get; set; }

        public string MaThanhToanCode { get; set; }
            = string.Empty;

        public string TenBenhNhan { get; set; }
            = string.Empty;

        public string TenDichVu { get; set; }
            = string.Empty;

        public DateTime NgayChamSoc { get; set; }

        public TimeSpan GioBatDau { get; set; }

        public TimeSpan GioKetThuc { get; set; }

        // Tổng giá trị dịch vụ.
        public decimal SoTien { get; set; }

        // Tổng số tiền khách đã trả.
        public decimal DaThanhToan { get; set; }

        public decimal ConLai =>
            Math.Max(0, SoTien - DaThanhToan);

        public string TrangThaiLich { get; set; }
            = string.Empty;

        public string TrangThaiThanhToan { get; set; }
            = "Chưa thanh toán";

        public string PhuongThucThanhToan { get; set; }
            = "Chưa chọn";

        public string LoaiThanhToan { get; set; }
            = string.Empty;

        public DateTime? NgayDatCoc { get; set; }

        public DateTime? NgayThanhToan { get; set; }

        public bool CoTheThanhToan =>
            TrangThaiLich != "Đã hủy"
            && TrangThaiThanhToan != "Đã thanh toán"
            && TrangThaiThanhToan != "Đã hoàn tiền"
            && TrangThaiThanhToan != "Không hoàn phí";

        public bool DaCoc =>
            TrangThaiThanhToan == "Đã đặt cọc";
    }

    public class CustomerPaymentDetailsViewModel
    {
        public int MaDatLich { get; set; }

        public int? MaThanhToan { get; set; }

        public string MaThanhToanCode { get; set; }
            = string.Empty;

        public string TenKhachHang { get; set; }
            = string.Empty;

        public string TenBenhNhan { get; set; }
            = string.Empty;

        public string TenDichVu { get; set; }
            = string.Empty;

        public string TenNguoiChamSoc { get; set; }
            = "Chưa phân công";

        public DateTime NgayChamSoc { get; set; }

        public TimeSpan GioBatDau { get; set; }

        public TimeSpan GioKetThuc { get; set; }

        public string DiaChiChamSoc { get; set; }
            = "Chưa cập nhật";

        // Tổng tiền dịch vụ.
        public decimal SoTien { get; set; }

        // Tiền khách đã trả.
        public decimal DaThanhToan { get; set; }

        public decimal ConLai =>
            Math.Max(0, SoTien - DaThanhToan);

        public decimal TyLeDatCoc { get; set; } = 30m;

        public string LoaiThanhToan { get; set; }
            = string.Empty;

        public string TrangThaiLich { get; set; }
            = string.Empty;

        public string TrangThaiThanhToan { get; set; }
            = "Chưa thanh toán";

        public string PhuongThucThanhToan { get; set; }
            = "Chưa chọn";

        public DateTime? NgayDatCoc { get; set; }

        public DateTime? NgayThanhToan { get; set; }

        public DateTime? NgayTaoThanhToan { get; set; }

        public bool CoTheThanhToan =>
            TrangThaiLich != "Đã hủy"
            && TrangThaiThanhToan != "Đã thanh toán"
            && TrangThaiThanhToan != "Đã hoàn tiền"
            && TrangThaiThanhToan != "Không hoàn phí";

        public bool DaCoc =>
            TrangThaiThanhToan == "Đã đặt cọc";
    }

    public class CustomerPaymentConfirmViewModel
    {
        public int MaDatLich { get; set; }

        public string TenBenhNhan { get; set; }
            = string.Empty;

        public string TenDichVu { get; set; }
            = string.Empty;

        public DateTime NgayChamSoc { get; set; }

        public TimeSpan GioBatDau { get; set; }

        public TimeSpan GioKetThuc { get; set; }

        // Tổng tiền dịch vụ.
        public decimal SoTien { get; set; }

        // Số khách đã trả trước đó.
        public decimal DaThanhToan { get; set; }

        // Số tiền cọc 30%.
        public decimal SoTienDatCoc { get; set; }

        // Số còn phải thanh toán.
        public decimal SoTienConLai { get; set; }

        public decimal TyLeDatCoc { get; set; } = 30m;

        public bool DaDatCoc { get; set; }

        [Required(
            ErrorMessage =
                "Vui lòng chọn hình thức thanh toán.")]
        public string LoaiThanhToan { get; set; }
            = string.Empty;

        [Required(
            ErrorMessage =
                "Vui lòng chọn phương thức thanh toán.")]
        [Display(Name = "Phương thức thanh toán")]
        public string PhuongThucThanhToan { get; set; }
            = string.Empty;
    }
}