using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CareConnect.Models
{
    [Table("ChungChi")]
    public class ChungChi
    {
        [Key]
        public int MaChungChi { get; set; }

        public int MaNguoiChamSoc { get; set; }


        [StringLength(200)]
        public string? TenChungChi { get; set; }


        [StringLength(200)]
        public string? DonViCap { get; set; }


        public DateTime? NgayCap { get; set; }


        public DateTime? NgayHetHan { get; set; }


        // =====================================
        // FILE SCAN / ẢNH / PDF
        // =====================================

        [StringLength(500)]
        public string? FileChungChi { get; set; }


        // =====================================
        // XÁC MINH
        // =====================================

        [StringLength(50)]
        public string TrangThaiXacMinh { get; set; }
            = "Chờ xác minh";


        public DateTime? NgayXacMinh { get; set; }


        [StringLength(500)]
        public string? GhiChuXacMinh { get; set; }


        [StringLength(100)]
        public string? PhuongThucXacMinh { get; set; }


        // =====================================
        // LIÊN KẾT NGƯỜI CHĂM SÓC
        // =====================================

        [ForeignKey(nameof(MaNguoiChamSoc))]
        public NguoiChamSoc? NguoiChamSoc { get; set; }
    }
}