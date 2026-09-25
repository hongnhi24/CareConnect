using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CareConnect.Models
{
    [Table("LichLamViec")]
    public class LichLamViec
    {
        [Key]
        public int MaLich { get; set; }

        public int MaNguoiChamSoc { get; set; }

        [Column(TypeName = "date")]
        public DateTime NgayLam { get; set; }

        [Column(TypeName = "time")]
        public TimeSpan GioBatDau { get; set; }

        [Column(TypeName = "time")]
        public TimeSpan GioKetThuc { get; set; }

        [StringLength(50)]
        public string? TrangThai { get; set; }

        [ForeignKey(nameof(MaNguoiChamSoc))]
        public NguoiChamSoc? NguoiChamSoc { get; set; }
    }
}
