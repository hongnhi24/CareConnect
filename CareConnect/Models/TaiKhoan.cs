using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CareConnect.Models
{
    [Table("TaiKhoan")]
    public class TaiKhoan
    {
        [Key]
        [Column("MaTaiKhoan")]
        public int MaTaiKhoan { get; set; }

        [Required]
        [StringLength(50)]
        [Column("TenDangNhap")]
        public string TenDangNhap { get; set; } = string.Empty;

        [Required]
        [StringLength(255)]
        [Column("MatKhau")]
        public string MatKhau { get; set; } = string.Empty;

        [StringLength(100)]
        [Column("Email")]
        public string? Email { get; set; }

        [Column("MaVaiTro")]
        public int MaVaiTro { get; set; }

        [Column("TrangThai")]
        public bool TrangThai { get; set; } = true;

        [Column("NgayTao")]
        public DateTime NgayTao { get; set; } = DateTime.Now;

        [ForeignKey(nameof(MaVaiTro))]
        public VaiTro? VaiTro { get; set; }
    }
}