using System.ComponentModel.DataAnnotations;

namespace CareConnect.ViewModels
{
    public class AdminSuggestionConfigViewModel
    {
        public int MaCauHinh { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập tên cấu hình.")]
        [StringLength(100)]
        [Display(Name = "Tên cấu hình")]
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
        public decimal DiemDanhGiaToiThieu { get; set; }

        [Range(0, 50)]
        public int KinhNghiemToiThieu { get; set; }

        public bool ChiGoiYNguoiDaDuyet { get; set; }

        public bool UuTienCungKhuVuc { get; set; }

        public bool DangApDung { get; set; }

        public DateTime NgayCapNhat { get; set; }

        public string? NguoiCapNhat { get; set; }

        public int TongTrongSo =>
            TrongSoKhuVuc
            + TrongSoChuyenMon
            + TrongSoKinhNghiem
            + TrongSoDanhGia
            + TrongSoMucGia
            + TrongSoLichTrong;
    }
}