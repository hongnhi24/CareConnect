using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CareConnect.Models
{
    [Table("KhachHang")]
    public class KhachHang
    {
        [Key]
        public int MaKhachHang { get; set; }

        public int MaTaiKhoan { get; set; }

        [StringLength(100)]
        public string HoTen { get; set; } = string.Empty;

        [StringLength(10)]
        public string? GioiTinh { get; set; }

        public DateTime? NgaySinh { get; set; }

        [StringLength(15)]
        public string? SoDienThoai { get; set; }

        [StringLength(100)]
        public string? Email { get; set; }

        [StringLength(255)]
        public string? DiaChi { get; set; }
        public bool TrangThaiHoatDong { get; set; } = true;

        public DateTime? NgayXoa { get; set; }
        [StringLength(12)]
        public string? CCCD { get; set; }

        public DateTime NgayTao { get; set; }

        [ForeignKey(nameof(MaTaiKhoan))]
        public TaiKhoan? TaiKhoan { get; set; }
    }
}