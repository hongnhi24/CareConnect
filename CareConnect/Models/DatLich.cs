using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CareConnect.Models
{
    [Table("DatLich")]
    public class DatLich
    {
        [Key]
        public int MaDatLich { get; set; }

        public int MaKhachHang { get; set; }

        public int MaBenhNhan { get; set; }

        public int? MaNguoiChamSoc { get; set; }
        [StringLength(50)]
        public string HinhThucPhanCong { get; set; }
    = "Tự động";

        [Column(TypeName = "decimal(5,2)")]
        public decimal? DiemPhuHopPhanCong { get; set; }

        public DateTime? NgayPhanCong { get; set; }
        public DateTime? ThoiDiemCheckIn { get; set; }

        public DateTime? ThoiDiemCheckOut { get; set; }
        public int MaDichVu { get; set; }

        public DateTime? NgayChamSoc { get; set; }

        public TimeSpan? GioBatDau { get; set; }

        public TimeSpan? GioKetThuc { get; set; }
        [StringLength(500)]
        public string? LyDoHuy { get; set; }

        [StringLength(100)]
        public string? NguoiHuy { get; set; }

        public DateTime? NgayHuy { get; set; }
        public string? DiaChiChamSoc { get; set; }

        public decimal? TongTien { get; set; }
        [Column(TypeName = "decimal(5,2)")]
        public decimal? TyLePhiNenTang { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? PhiNenTang { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? ThuNhapNguoiChamSoc { get; set; }

        public string? TrangThai { get; set; }

        public string? GhiChu { get; set; }

        public DateTime NgayDat { get; set; }

        [ForeignKey(nameof(MaKhachHang))]
        public KhachHang? KhachHang { get; set; }

        [ForeignKey(nameof(MaBenhNhan))]
        public BenhNhan? BenhNhan { get; set; }

        [ForeignKey(nameof(MaNguoiChamSoc))]
        public NguoiChamSoc? NguoiChamSoc { get; set; }
        public NhatKyChamSoc? NhatKyChamSoc { get; set; }
        public ThanhToan? ThanhToan { get; set; }
        public DanhGia? DanhGia { get; set; }
    }
}