using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace CareConnect.ViewModels
{
    public class CaregiverCertificateFileViewModel
    {
        public int MaChungChi { get; set; }


        public string TenChungChi { get; set; }
            = string.Empty;


        public string? DonViCap { get; set; }


        [Required(
            ErrorMessage = "Vui lòng chọn file chứng chỉ.")]
        public IFormFile? FileChungChi { get; set; }
    }
}