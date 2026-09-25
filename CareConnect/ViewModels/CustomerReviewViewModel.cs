using System.ComponentModel.DataAnnotations;

namespace CareConnect.ViewModels
{
    public class CustomerReviewListViewModel
    {
        public int TongLichCoTheDanhGia { get; set; }

        public int ChuaDanhGia { get; set; }

        public int DaDanhGia { get; set; }

        public double DiemTrungBinh { get; set; }

        public string TrangThai { get; set; }
            = string.Empty;

        public List<CustomerReviewItemViewModel>
            DanhSach
        { get; set; } = new();
    }

    public class CustomerReviewItemViewModel
    {
        public int MaDatLich { get; set; }

        public int? MaDanhGia { get; set; }

        public string TenBenhNhan { get; set; }
            = string.Empty;

        public string TenNguoiChamSoc { get; set; }
            = "Chưa phân công";

        public string TenDichVu { get; set; }
            = string.Empty;

        public DateTime NgayChamSoc { get; set; }

        public TimeSpan GioBatDau { get; set; }

        public TimeSpan GioKetThuc { get; set; }

        public string TrangThaiLich { get; set; }
            = string.Empty;

        public int? SoSao { get; set; }

        public string NoiDung { get; set; }
            = string.Empty;

        public DateTime? NgayDanhGia { get; set; }

        public bool DaDanhGia =>
            MaDanhGia.HasValue;

        public bool CoTheDanhGia =>
            TrangThaiLich == "Đã hoàn thành"
            &&
            !DaDanhGia;
    }

    public class CustomerReviewCreateViewModel
    {
        public int MaDatLich { get; set; }

        public string TenBenhNhan { get; set; }
            = string.Empty;

        public string TenNguoiChamSoc { get; set; }
            = "Chưa cập nhật";

        public string TenDichVu { get; set; }
            = string.Empty;

        public DateTime NgayChamSoc { get; set; }

        public TimeSpan GioBatDau { get; set; }

        public TimeSpan GioKetThuc { get; set; }

        [Required(
            ErrorMessage =
                "Vui lòng chọn số sao đánh giá.")]
        [Range(
            1,
            5,
            ErrorMessage =
                "Số sao phải từ 1 đến 5.")]
        [Display(Name = "Mức độ hài lòng")]
        public int? SoSao { get; set; }

        [Required(
            ErrorMessage =
                "Vui lòng nhập nội dung đánh giá.")]
        [StringLength(
            1000,
            MinimumLength = 10,
            ErrorMessage =
                "Nội dung đánh giá phải từ 10 đến 1000 ký tự.")]
        [Display(Name = "Nội dung đánh giá")]
        public string NoiDung { get; set; }
            = string.Empty;
    }

    public class CustomerReviewDetailsViewModel
    {
        public int MaDanhGia { get; set; }

        public int MaDatLich { get; set; }

        public string TenBenhNhan { get; set; }
            = string.Empty;

        public string TenNguoiChamSoc { get; set; }
            = "Chưa cập nhật";

        public string TenDichVu { get; set; }
            = string.Empty;

        public DateTime NgayChamSoc { get; set; }

        public TimeSpan GioBatDau { get; set; }

        public TimeSpan GioKetThuc { get; set; }

        public int SoSao { get; set; }

        public string NoiDung { get; set; }
            = string.Empty;

        public DateTime NgayDanhGia { get; set; }
    }
    public class CustomerReviewEditViewModel
    {
        public int MaDanhGia { get; set; }

        public int MaDatLich { get; set; }

        public string TenBenhNhan { get; set; }
            = string.Empty;

        public string TenNguoiChamSoc { get; set; }
            = "Chưa cập nhật";

        public string TenDichVu { get; set; }
            = string.Empty;

        public DateTime NgayChamSoc { get; set; }

        public TimeSpan GioBatDau { get; set; }

        public TimeSpan GioKetThuc { get; set; }

        [Required(
            ErrorMessage = "Vui lòng chọn số sao đánh giá.")]
        [Range(
            1,
            5,
            ErrorMessage = "Số sao phải từ 1 đến 5.")]
        [Display(Name = "Mức độ hài lòng")]
        public int? SoSao { get; set; }

        [Required(
            ErrorMessage = "Vui lòng nhập nội dung đánh giá.")]
        [StringLength(
            1000,
            MinimumLength = 10,
            ErrorMessage =
                "Nội dung đánh giá phải từ 10 đến 1000 ký tự.")]
        [Display(Name = "Nội dung đánh giá")]
        public string NoiDung { get; set; }
            = string.Empty;
    }
}