using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CareConnect.Models
{
    [Table("BenhNhan")]
    public class BenhNhan
    {
        [Key]
        public int MaBenhNhan { get; set; }

        public int MaKhachHang { get; set; }

        [StringLength(30)]
        public string? MaBenhNhanCode { get; set; }

        [Required]
        [StringLength(100)]
        public string HoTen { get; set; }
            = string.Empty;

        [StringLength(20)]
        public string? GioiTinh { get; set; }

        public DateTime? NgaySinh { get; set; }

        [StringLength(20)]
        public string? SoDienThoai { get; set; }

        [StringLength(300)]
        public string? DiaChi { get; set; }

        [StringLength(1000)]
        public string? TinhTrangSucKhoe { get; set; }

        [StringLength(1000)]
        public string? TienSuBenh { get; set; }

        [StringLength(100)]
        public string? NguoiLienHeKhanCap { get; set; }

        [StringLength(20)]
        public string? SoDienThoaiKhanCap { get; set; }

        [StringLength(1000)]
        public string? GhiChu { get; set; }

        [StringLength(50)]
        public string? QuanHe { get; set; }

        [StringLength(500)]
        public string? DiUngThuoc { get; set; }

        [StringLength(20)]
        public string? NhomMau { get; set; }

        public DateTime NgayTao { get; set; }
        public bool TrangThaiHoatDong { get; set; } = true;

        public DateTime? NgayXoa { get; set; }

        [ForeignKey(nameof(MaKhachHang))]
        public KhachHang? KhachHang { get; set; }
    }
}