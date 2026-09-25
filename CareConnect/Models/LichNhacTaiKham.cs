using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CareConnect.Models
{
    [Table("LichNhacTaiKham")]
    public class LichNhacTaiKham
    {
        [Key]
        public int MaLichNhacTaiKham { get; set; }

        public int MaBenhNhan { get; set; }

        [Required]
        public DateTime NgayTaiKham { get; set; }

        [Required]
        public TimeSpan GioTaiKham { get; set; }

        [Required]
        [StringLength(300)]
        public string NoiDung { get; set; } = string.Empty;

        [StringLength(255)]
        public string? DiaDiem { get; set; }

        [StringLength(500)]
        public string? GhiChu { get; set; }

        [Range(5, 10080)]
        public int SoPhutNhacTruoc { get; set; } = 1440;

        public bool TrangThai { get; set; } = true;

        public DateTime NgayTao { get; set; }

        public DateTime? LanNhacGanNhat { get; set; }

        [ForeignKey(nameof(MaBenhNhan))]
        public BenhNhan? BenhNhan { get; set; }
    }
}
