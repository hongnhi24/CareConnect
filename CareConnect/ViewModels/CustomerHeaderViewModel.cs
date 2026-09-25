namespace CareConnect.ViewModels
{
    public class CustomerHeaderViewModel
    {
        public string TenNguoiDung { get; set; }
            = "Khách hàng";

        public string VietTat { get; set; }
            = "KH";

        public int SoThongBaoChuaDoc { get; set; }

        public List<CustomerHeaderNotificationViewModel>
            ThongBaoGanDay
        {
            get;
            set;
        } = new();
    }

    public class CustomerHeaderNotificationViewModel
    {
        public int MaThongBao { get; set; }

        public string TieuDe { get; set; }
            = "Thông báo";

        public string NoiDung { get; set; }
            = string.Empty;

        public bool DaDoc { get; set; }

        public DateTime NgayGui { get; set; }
    }
}