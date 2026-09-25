namespace CareConnect.Constants
{
    public static class PaymentPolicy
    {
        // Khách hàng có thể đặt cọc 30%
        public const decimal DepositPercent = 30m;

        // CareConnect giữ lại 15% phí nền tảng
        public const decimal PlatformFeePercent = 15m;

        // Người chăm sóc nhận 85%
        public const decimal CaregiverPercent =
            100m - PlatformFeePercent;
    }
}