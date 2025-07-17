namespace Neptune.NsPay.Common
{
    public static class CultureTimeHelper
    {
        public static string TimeCodeZhCN = "zh-CN";
        public static string TimeCodeViVn = "vi-VN";
        public static string TimeCodeEST = "EST";

        public static DateTime GetCultureTimeInfo(DateTime dateTime,string countryCode)
        {
            try
            {
                DateTime time = dateTime;
                if (countryCode == TimeCodeZhCN)
                {
                    time = TimeZoneInfo.ConvertTimeBySystemTimeZoneId(dateTime, "China Standard Time");
                }
                if (countryCode == TimeCodeViVn)
                {
                    //time = dateTime.AddHours(-1);
                    TimeZoneInfo vietnamTimeZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
                    time = TimeZoneInfo.ConvertTime(dateTime, vietnamTimeZone);
                }
                if (countryCode.ToUpper() == TimeCodeEST)
                {
                    time = dateTime.AddHours(-12);
                }
                return time;
            }
            catch (Exception)
            {
                return dateTime;
            }
        }

        public static DateTime GetCultureTimeInfoByGTM(DateTime dateTime,string gtm)
        {
            if (gtm == "GMT8+")
            {
                dateTime = CultureTimeHelper.GetCultureTimeInfo(dateTime, CultureTimeHelper.TimeCodeZhCN);
            }
            if (gtm == "GMT7+")
            {
                dateTime = dateTime.AddHours(1);
            }
            if (gtm == "GMT4-")
            {
                dateTime = dateTime.AddHours(12);
            }
            return dateTime;
        }
    }
}
