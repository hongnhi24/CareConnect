using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace CareConnect.ViewModels
{
    public class CaregiverCertificateViewModel
    {
        [Required(
            ErrorMessage = "Vui lòng nhập tên chứng chỉ.")]
        [StringLength(200)]
        [Display(Name = "Tên chứng chỉ")]
        public string TenChungChi { get; set; }
            = string.Empty;


        [StringLength(200)]
        [Display(Name = "Đơn vị cấp")]
        public string? DonViCap { get; set; }


        [DataType(DataType.Date)]
        [Display(Name = "Ngày cấp")]
        public DateTime? NgayCap { get; set; }


        [DataType(DataType.Date)]
        [Display(Name = "Ngày hết hạn")]
        public DateTime? NgayHetHan { get; set; }


        [Required(
            ErrorMessage = "Vui lòng tải file chứng chỉ.")]
        [Display(Name = "Ảnh hoặc PDF chứng chỉ")]
        public IFormFile? FileChungChi { get; set; }
    }
}