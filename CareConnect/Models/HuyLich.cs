using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CareConnect.Models
{
    [Table("HuyLich")]
    public class HuyLich
    {
        [Key]
        public int MaHuyLich { get; set; }


        public int MaDatLich { get; set; }


        public int MaKhachHang { get; set; }


        [Required]
        [StringLength(100)]
        public string LyDo { get; set; }
            = string.Empty;


        [StringLength(500)]
        public string? GhiChu { get; set; }


        public DateTime ThoiDiemHuy { get; set; }
            = DateTime.Now;


        [Required]
        [StringLength(50)]
        public string TrangThaiLucHuy { get; set; }
            = string.Empty;


        [Column(TypeName = "decimal(18,2)")]
        public decimal TongTien { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal SoTienDaThanhToan { get; set; }

        [Column(TypeName = "decimal(5,2)")]
        public decimal TyLeHoan { get; set; }


        [Column(TypeName = "decimal(18,2)")]
        public decimal SoTienHoan { get; set; }


        [Required]
        [StringLength(50)]
        public string TrangThaiHoanTien { get; set; }
            = "Chưa xử lý";


        public bool HuyGiuaChung { get; set; }


        [ForeignKey(nameof(MaDatLich))]
        public DatLich? DatLich { get; set; }


        [ForeignKey(nameof(MaKhachHang))]
        public KhachHang? KhachHang { get; set; }
    }
}