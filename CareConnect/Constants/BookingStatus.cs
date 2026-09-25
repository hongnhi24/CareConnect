namespace CareConnect.Constants
{
    public static class BookingStatus
    {
        public const string ChoXacNhan =
            "Chờ xác nhận";

        public const string DaXacNhan =
            "Đã xác nhận";

        public const string DangThucHien =
            "Đang thực hiện";

        public const string DaHoanThanh =
            "Đã hoàn thành";

        public const string DaHuy =
            "Đã hủy";

        public static readonly string[] TatCa =
        {
            ChoXacNhan,
            DaXacNhan,
            DangThucHien,
            DaHoanThanh,
            DaHuy
        };
    }
}