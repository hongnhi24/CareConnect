namespace CareConnect.Services
{
    public static class VietnamClock
    {
        private static readonly TimeZoneInfo
            VietnamTimeZone =
                ResolveTimeZone();

        public static DateTime Now
        {
            get
            {
                return TimeZoneInfo
                    .ConvertTimeFromUtc(
                        DateTime.UtcNow,
                        VietnamTimeZone);
            }
        }


        private static TimeZoneInfo ResolveTimeZone()
        {
            string[] ids =
            {
                "Asia/Ho_Chi_Minh",
                "SE Asia Standard Time"
            };

            foreach (string id in ids)
            {
                try
                {
                    return TimeZoneInfo
                        .FindSystemTimeZoneById(id);
                }
                catch (TimeZoneNotFoundException)
                {
                }
                catch (InvalidTimeZoneException)
                {
                }
            }

            return TimeZoneInfo.Local;
        }
    }
}