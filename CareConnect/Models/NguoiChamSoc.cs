using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CareConnect.Models
{
    [Table("NguoiChamSoc")]
    public class NguoiChamSoc
    {
        [Key]
        public int MaNguoiChamSoc { get; set; }

        public int? MaTaiKhoan { get; set; }

        public string? HoTen { get; set; }

        public string? GioiTinh { get; set; }

        public DateTime? NgaySinh { get; set; }

        public string? SoDienThoai { get; set; }

        public string? Email { get; set; }

        public string? DiaChi { get; set; }

        public string? BangCap { get; set; }

        public int? KinhNghiem { get; set; }

        public string? ChuyenMon { get; set; }

        public decimal? GiaTheoGio { get; set; }

        public double? DanhGia { get; set; }

        public string? TrangThai { get; set; }

        public DateTime NgayTao { get; set; }

        public string? KhuVucHoatDong { get; set; }

        public string? GioiThieu { get; set; }
    }
}