namespace CareConnect.ViewModels
{
    public class CaregiverHeaderViewModel
    {
        public string HoTen { get; set; }
            = "Người chăm sóc";

        public string VietTat { get; set; }
            = "NCS";

        public int SoThongBaoChuaDoc { get; set; }

        public List<CaregiverHeaderNotificationViewModel>
            ThongBaoGanDay
        {
            get;
            set;
        } = new();
    }

    public class CaregiverHeaderNotificationViewModel
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