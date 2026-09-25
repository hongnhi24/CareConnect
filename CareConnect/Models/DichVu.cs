using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CareConnect.Models
{
    [Table("DichVu")]
    public class DichVu
    {
        [Key]
        public int MaDichVu { get; set; }

        [Required]
        [StringLength(150)]
        public string TenDichVu { get; set; } = string.Empty;

        [StringLength(1000)]
        public string? MoTa { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Gia { get; set; }

        public int ThoiLuong { get; set; }

        public bool TrangThai { get; set; }

        public DateTime NgayTao { get; set; }

        [StringLength(20)]
        public string MaDichVuCode { get; set; } = string.Empty;
    }
}