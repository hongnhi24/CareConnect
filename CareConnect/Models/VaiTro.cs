using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CareConnect.Models
{
    [Table("VaiTro")]
    public class VaiTro
    {
        [Key]
        [Column("MaVaiTro")]
        public int MaVaiTro { get; set; }

        [Required]
        [StringLength(50)]
        [Column("TenVaiTro")]
        public string TenVaiTro { get; set; } = string.Empty;

        public ICollection<TaiKhoan> TaiKhoans { get; set; }
            = new List<TaiKhoan>();
    }
}