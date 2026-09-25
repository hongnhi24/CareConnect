using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CareConnect.Models
{
    [Table("LichNhacThuoc")]
    public class LichNhacThuoc
    {
        [Key]
        public int MaLichNhac { get; set; }

        public int MaBenhNhan { get; set; }

        [Required]
        [StringLength(200)]
        public string TenThuoc { get; set; } = string.Empty;

        [StringLength(100)]
        public string? LieuDung { get; set; }

        [StringLength(255)]
        public string? CachDung { get; set; }

        public TimeSpan ThoiGianUong { get; set; }

        public DateTime NgayBatDau { get; set; }

        public DateTime? NgayKetThuc { get; set; }

        [Required]
        [StringLength(50)]
        public string TanSuat { get; set; } = "Hằng ngày";

        [StringLength(500)]
        public string? GhiChu { get; set; }

        public bool TrangThai { get; set; } = true;

        public DateTime NgayTao { get; set; }

        public DateTime? LanNhacGanNhat { get; set; }

        [ForeignKey(nameof(MaBenhNhan))]
        public BenhNhan? BenhNhan { get; set; }
    }
}