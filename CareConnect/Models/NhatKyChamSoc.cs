using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CareConnect.Models
{
    [Table("NhatKyChamSoc")]
    public class NhatKyChamSoc
    {
        [Key]
        public int MaNhatKy { get; set; }

        public int MaDatLich { get; set; }

        [StringLength(1500)]
        public string? TinhTrangSucKhoe { get; set; }

        [StringLength(30)]
        public string? HuyetAp { get; set; }

        public int? NhipTim { get; set; }

        [Column(TypeName = "decimal(4,1)")]
        public decimal? NhietDo { get; set; }

        [Column(TypeName = "decimal(6,2)")]
        public decimal? CanNang { get; set; }

        [StringLength(500)]
        public string? TinhTrangAnUong { get; set; }

        [StringLength(500)]
        public string? ThuocDaUong { get; set; }

        [StringLength(1500)]
        public string? GhiChu { get; set; }

        public DateTime? NgayCapNhat { get; set; }

        [ForeignKey(nameof(MaDatLich))]
        public DatLich? DatLich { get; set; }
    }
}