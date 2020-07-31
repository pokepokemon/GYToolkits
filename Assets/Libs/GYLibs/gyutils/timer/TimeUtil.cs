using System;

namespace GYLib.Utils
{
    public class TimeUtil
    {
        /// <summary>
        /// 供外部设置的时间数据
        /// Time.realtimeSinceStartup
        /// </summary>
        public static float shareRealTimeSincePlay;

        public static string FormatCountdownString(int seconds, string dayStr = "天")
        {
            if (seconds <= 0)
            {
                return "00:00:00";
            }

            string output = "";

            int day = (int)Math.Floor((double)(seconds / 86400));
            if (day > 1)
            {
                output = day + dayStr;
            }
            else
            {
                int hours = (int) Math.Floor((double) (seconds / 3600));
                int mins = (int)Math.Floor((double)((seconds - hours * 3600) / 60));
                int secs = seconds % 60;

                output = string.Format("{0:00}:{1:00}:{2:00}", hours, mins, secs);
            }
            return output;
        }

        public static string FormatCountdownString(long seconds, string dayStr = "天")
        {
            if (seconds <= 0)
            {
                return "00:00:00";
            }

            string output = "";

            int day = (int)Math.Floor((double)(seconds / 86400));
            if (day > 1)
            {
                output = day + dayStr;
            }
            else
            {
                int hours = (int)Math.Floor((double)(seconds / 3600));
                int mins = (int)Math.Floor((double)((seconds - hours * 3600) / 60));
                long secs = seconds % 60;

                output = string.Format("{0:00}:{1:00}:{2:00}", hours, mins, secs);
            }
            return output;
        }

        public static string FormatCountdownString(uint seconds, string dayStr = "天")
        {
            if (seconds <= 0)
            {
                return "00:00:00";
            }

            string output = "";

            int day = (int)Math.Floor((double)(seconds / 86400));
            if (day > 1)
            {
                output = day + dayStr;
            }
            else
            {
                int hours = (int)Math.Floor((double)(seconds / 3600));
                int mins = (int)Math.Floor((double)((seconds - hours * 3600) / 60));
                long secs = seconds % 60;

                output = string.Format("{0:00}:{1:00}:{2:00}", hours, mins, secs);
            }
            return output;
        }

        public static double ConvertDateTimeInt(DateTime time)
        {
            double intResult = 0;
            DateTime startTime = TimeZone.CurrentTimeZone.ToLocalTime(new DateTime(1970, 1, 1));
            intResult = (time - startTime).TotalSeconds;
            return intResult;
        }

        public static DateTime ConvertIntDateTime(double d)
        {
            DateTime time = DateTime.MinValue;
            DateTime startTime = TimeZone.CurrentTimeZone.ToLocalTime(new DateTime(1970, 1, 1));
            time = startTime.AddSeconds(d);
            return time;
        }

        /// <summary>
        /// 单位毫秒
        /// </summary>
        /// <returns></returns>
        public static long GetTimestamp()
        {
            return GetTimestamp(DateTime.Now);
        }
        public static long GetTimestamp(DateTime dateTime)
        {
            DateTime dt1970 = new DateTime(1970, 1, 1, 0, 0, 0, 0);//默认为北京时间
            return (dateTime.Ticks - dt1970.Ticks) / 10000;
        }

        /// 单位毫秒
        public static DateTime GetDateTime(long timestamp)
        {
            DateTime dt1970 = new DateTime(1970, 1, 1, 0, 0, 0, 0);
            long t = dt1970.Ticks + timestamp * 10000;
            return new DateTime(t);
        }

        /// <summary>
        /// 2019年到现在的Utc时间(秒)
        /// </summary>
        /// <returns></returns>
        public static long GetCurrentSec2019()
        {
            TimeSpan ts = DateTime.UtcNow - new DateTime(2019, 1, 1, 0, 0, 0, 0);
            return Convert.ToInt64(ts.TotalSeconds);
        }

        public static DateTime GetDateBySec2019(long sec)
        {
            DateTime date2019 = new DateTime(2019, 1, 1, 0, 0, 0, 0);
            date2019.AddSeconds(sec);
            return date2019;
        }

        /// <summary>
        /// 获取2019年到今天的UTC时间(秒)
        /// </summary>
        /// <returns></returns>
        public static long GetTodaySec2019()
        {
            DateTime now = DateTime.UtcNow;
            DateTime today = new DateTime(now.Year, now.Month, now.Day);
            TimeSpan ts = today - new DateTime(2019, 1, 1, 0, 0, 0, 0);
            return Convert.ToInt64(ts.TotalSeconds);
        }

        /// <summary>
        /// 获取Utc当前时间
        /// </summary>
        /// <returns></returns>
        public static DateTime GetCurrentDate()
        {
            return DateTime.UtcNow;
        }

        /// <summary>
        /// 获取当地2019年到某天的UTC秒数
        /// </summary>
        /// <param name="year"></param>
        /// <param name="month"></param>
        /// <param name="day"></param>
        /// <returns></returns>
        public static long GetSec2019ByDate(int year, int month, int day)
        {
            DateTime thisday = new DateTime(year, month, day);
            TimeSpan ts = thisday - new DateTime(2019, 1, 1, 0, 0, 0, 0);
            return Convert.ToInt64(ts.TotalSeconds);
        }

        /*
        public static string FormatPastTimeString(uint timestamp)
        {
        	double curTime = ServerTime.Instance.CurrTime;
        	double timeDuration = curTime - timestamp;

        	if (timeDuration < 60)
        	{
        		return "刚刚";
        	}
        	else if (timeDuration >= 60 && timeDuration < 3600)
        	{
        		int mins = (int)(timeDuration / 60);
        		return mins + "分钟前";
        	}
        	else if (timeDuration >= 3600 && timeDuration < 3600.0 * 24.0)
        	{
        		int hours = (int)(timeDuration / 3600);

        		return hours + "小时前";
        	}


			int days = (int)(timeDuration / (3600 * 24));
			return days + "天前";
        }*/
    }
}
