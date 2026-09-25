using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CareConnect.Models
{
    [Table("ThanhToan")]
    public class ThanhToan
    {
        [Key]
        public int MaThanhToan { get; set; }

        public int MaDatLich { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal SoTien { get; set; }
        [Column(TypeName = "decimal(5,2)")]
        public decimal TyLeDatCoc { get; set; } = 30m;

        [StringLength(50)]
        public string? LoaiThanhToan { get; set; }

        public DateTime? NgayDatCoc { get; set; }

        [StringLength(100)]
        public string? PhuongThucThanhToan { get; set; }

        [StringLength(100)]
        public string TrangThai { get; set; }
            = "Chưa thanh toán";

        public DateTime? NgayThanhToan { get; set; }

        public DateTime NgayTao { get; set; }

        [DatabaseGenerated(
            DatabaseGeneratedOption.Computed)]
        public string? MaThanhToanCode { get; set; }

        [ForeignKey(nameof(MaDatLich))]
        public DatLich? DatLich { get; set; }
    }
}