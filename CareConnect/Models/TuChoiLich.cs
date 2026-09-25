using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CareConnect.Models
{
    [Table("TuChoiLich")]
    public class TuChoiLich
    {
        [Key]
        public int MaTuChoi { get; set; }

        public int MaDatLich { get; set; }

        public int MaNguoiChamSoc { get; set; }

        [Required]
        [StringLength(100)]
        public string LyDo { get; set; } = string.Empty;

        [StringLength(500)]
        public string? GhiChu { get; set; }

        public DateTime NgayTuChoi { get; set; } = DateTime.Now;


        [ForeignKey(nameof(MaDatLich))]
        public DatLich? DatLich { get; set; }


        [ForeignKey(nameof(MaNguoiChamSoc))]
        public NguoiChamSoc? NguoiChamSoc { get; set; }
    }
}