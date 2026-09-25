namespace CareConnect.ViewModels
{
    public class CaregiverProfileCompletenessViewModel
    {
        public bool HoSoDayDu { get; set; }

        public int TongSoMuc { get; set; }

        public int SoMucDaHoanThanh { get; set; }

        public int PhanTramHoanThanh { get; set; }

        public List<string> ThieuThongTin { get; set; }
            = new();
    }
}