using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CareConnect.Models
{
    [Table("CauHinhGoiY")]
    public class CauHinhGoiY
    {
        [Key]
        public int MaCauHinh { get; set; }

        [Required]
        [StringLength(100)]
        public string TenCauHinh { get; set; } = string.Empty;

        [Range(0, 100)]
        public int TrongSoKhuVuc { get; set; }

        [Range(0, 100)]
        public int TrongSoChuyenMon { get; set; }

        [Range(0, 100)]
        public int TrongSoKinhNghiem { get; set; }

        [Range(0, 100)]
        public int TrongSoDanhGia { get; set; }

        [Range(0, 100)]
        public int TrongSoMucGia { get; set; }

        [Range(0, 100)]
        public int TrongSoLichTrong { get; set; }

        [Range(1, 20)]
        public int SoLuongGoiY { get; set; }

        [Range(0, 5)]
        [Column(TypeName = "decimal(3,2)")]
        public decimal DiemDanhGiaToiThieu { get; set; }

        [Range(0, 50)]
        public int KinhNghiemToiThieu { get; set; }

        public bool ChiGoiYNguoiDaDuyet { get; set; }

        public bool UuTienCungKhuVuc { get; set; }

        public bool DangApDung { get; set; }

        public DateTime NgayCapNhat { get; set; }

        [StringLength(100)]
        public string? NguoiCapNhat { get; set; }
    }
}