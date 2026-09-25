namespace CareConnect.ViewModels
{
    public class CustomerCareJournalListViewModel
    {
        public int TongNhatKy { get; set; }

        public int TongNguoiThan { get; set; }

        public string TuKhoa { get; set; }
            = string.Empty;

        public int? MaBenhNhan { get; set; }

        public List<CustomerCareJournalItemViewModel>
            DanhSach
        { get; set; } = new();

        public List<CustomerJournalPatientOptionViewModel>
            DanhSachBenhNhan
        { get; set; } = new();
    }

    public class CustomerJournalPatientOptionViewModel
    {
        public int MaBenhNhan { get; set; }

        public string HoTen { get; set; }
            = string.Empty;
    }

    public class CustomerCareJournalItemViewModel
    {
        public int MaNhatKy { get; set; }

        public int MaDatLich { get; set; }

        public int MaBenhNhan { get; set; }

        public string TenBenhNhan { get; set; }
            = string.Empty;

        public string TenNguoiChamSoc { get; set; }
            = "Chưa phân công";

        public string TenDichVu { get; set; }
            = string.Empty;

        public DateTime NgayChamSoc { get; set; }

        public TimeSpan GioBatDau { get; set; }

        public TimeSpan GioKetThuc { get; set; }

        public string TinhTrangSucKhoe { get; set; }
            = "Chưa cập nhật";

        public string HuyetAp { get; set; }
            = "Chưa cập nhật";

        public int? NhipTim { get; set; }

        public decimal? NhietDo { get; set; }

        public decimal? CanNang { get; set; }

        public string TinhTrangAnUong { get; set; }
            = "Chưa cập nhật";

        public string ThuocDaUong { get; set; }
            = "Chưa cập nhật";

        public string GhiChu { get; set; }
            = "Không có ghi chú.";

        public DateTime? NgayCapNhat { get; set; }
    }

    public class CustomerCareJournalDetailsViewModel
        : CustomerCareJournalItemViewModel
    {
        public string DiaChiChamSoc { get; set; }
            = "Chưa cập nhật";

        public string TrangThaiLich { get; set; }
            = string.Empty;
    }
}