using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CareConnect.Models
{
    [Table("DanhGia")]
    public class DanhGia
    {
        [Key]
        public int MaDanhGia { get; set; }

        public int MaDatLich { get; set; }

        public int SoSao { get; set; }

        public string? NoiDung { get; set; }

        public DateTime NgayDanhGia { get; set; }

        [ForeignKey(nameof(MaDatLich))]
        public DatLich? DatLich { get; set; }
    }
}