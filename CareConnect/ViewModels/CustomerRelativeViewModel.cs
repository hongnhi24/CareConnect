using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace CareConnect.ViewModels
{
    public class CustomerRelativeListViewModel
    {
        public int TongHoSo { get; set; }

        public string TuKhoa { get; set; }
            = string.Empty;

        public List<CustomerRelativeItemViewModel>
            DanhSachNguoiThan
        { get; set; }
                = new();
    }

    public class CustomerRelativeItemViewModel
    {
        public int MaBenhNhan { get; set; }

        public string MaBenhNhanCode { get; set; }
            = string.Empty;

        public string HoTen { get; set; }
            = string.Empty;

        public string GioiTinh { get; set; }
            = "Chưa cập nhật";

        public DateTime? NgaySinh { get; set; }

        public string SoDienThoai { get; set; }
            = "Chưa cập nhật";

        public string DiaChi { get; set; }
            = "Chưa cập nhật";

        public string TinhTrangSucKhoe { get; set; }
            = "Chưa cập nhật";

        public string TienSuBenh { get; set; }
            = "Chưa cập nhật";

        public string NhomMau { get; set; }
            = "Chưa cập nhật";

        public string DiUngThuoc { get; set; }
            = "Không ghi nhận";

        public string QuanHe { get; set; }
            = "Người thân";

        public string? GhiChu { get; set; }

        public int Tuoi
        {
            get
            {
                DateTime? ngaySinh = NgaySinh;

                if (!ngaySinh.HasValue)
                {
                    return 0;
                }

                DateTime homNay = DateTime.Today;

                int tuoi =
                    homNay.Year
                    - ngaySinh.Value.Year;

                if (ngaySinh.Value.Date
                    > homNay.AddYears(-tuoi))
                {
                    tuoi--;
                }

                return tuoi < 0
                    ? 0
                    : tuoi;
            }
        }
    }

    public class CustomerRelativeFormViewModel
    {
        public int MaBenhNhan { get; set; }

        public string? MaBenhNhanCode { get; set; }

        [Required(
            ErrorMessage =
                "Vui lòng nhập họ và tên.")]
        [StringLength(100)]
        [Display(Name = "Họ và tên")]
        public string HoTen { get; set; }
            = string.Empty;

        [Display(Name = "Giới tính")]
        public string? GioiTinh { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "Ngày sinh")]
        public DateTime? NgaySinh { get; set; }

        [Phone(
            ErrorMessage =
                "Số điện thoại không hợp lệ.")]
        [StringLength(20)]
        [Display(Name = "Số điện thoại")]
        public string? SoDienThoai { get; set; }

        [StringLength(300)]
        [Display(Name = "Địa chỉ")]
        public string? DiaChi { get; set; }

        [StringLength(1000)]
        [Display(Name = "Tình trạng sức khỏe")]
        public string? TinhTrangSucKhoe { get; set; }

        [StringLength(1000)]
        [Display(Name = "Tiền sử bệnh")]
        public string? TienSuBenh { get; set; }

        [StringLength(20)]
        [Display(Name = "Nhóm máu")]
        public string? NhomMau { get; set; }

        [StringLength(500)]
        [Display(Name = "Dị ứng thuốc")]
        public string? DiUngThuoc { get; set; }

        [StringLength(100)]
        [Display(Name = "Người liên hệ khẩn cấp")]
        public string? NguoiLienHeKhanCap { get; set; }

        [Phone(
            ErrorMessage =
                "Số điện thoại khẩn cấp không hợp lệ.")]
        [StringLength(20)]
        [Display(Name = "Số điện thoại khẩn cấp")]
        public string? SoDienThoaiKhanCap { get; set; }

        [StringLength(50)]
        [Display(Name = "Quan hệ")]
        public string? QuanHe { get; set; }

        [StringLength(1000)]
        [Display(Name = "Ghi chú")]
        public string? GhiChu { get; set; }
    }

    public class CustomerRelativeDetailsViewModel
    : CustomerRelativeFormViewModel
    {
        public int SoLichChamSoc { get; set; }

        public DateTime NgayTao { get; set; }

        public int Tuoi
        {
            get
            {
                if (!NgaySinh.HasValue)
                {
                    return 0;
                }

                DateTime homNay =
                    DateTime.Today;

                int tuoi =
                    homNay.Year
                    - NgaySinh.Value.Year;

                if (NgaySinh.Value.Date
                    > homNay.AddYears(-tuoi))
                {
                    tuoi--;
                }

                return tuoi < 0
                    ? 0
                    : tuoi;
            }
        }
    }
}