using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CareConnect.Models
{
    [Table("ThongBao")]
    public class ThongBao
    {
        [Key]
        public int MaThongBao { get; set; }

        public int MaTaiKhoan { get; set; }

        [StringLength(200)]
        public string TieuDe { get; set; }
            = string.Empty;

        [StringLength(1000)]
        public string NoiDung { get; set; }
            = string.Empty;

        public bool DaDoc { get; set; }

        public DateTime NgayGui { get; set; }

        [ForeignKey(nameof(MaTaiKhoan))]
        public TaiKhoan? TaiKhoan { get; set; }
    }
}