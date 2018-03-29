using System;

namespace UniLua
{
	using System.Diagnostics;

	internal class LuaOSLib
	{
		public const string LIB_NAME = "os";
        private static string[] Month = new string[]{"January", "February", "March", "April", "May", "June", "July", "August", "September", "October", "November", "December"};

		public static int OpenLib( ILuaState lua )
		{
			NameFuncPair[] define = new NameFuncPair[]
			{
#if !UNITY_WEBPLAYER
				new NameFuncPair("clock", OS_Clock),
                new NameFuncPair("utctime", OS_UtcTime),
                new NameFuncPair("time", OS_Time),
                new NameFuncPair("touint64", STRING2UINT64),
                new NameFuncPair("date", OS_Date),
                new NameFuncPair("utcdate", OS_UtcDate),
#endif
			};

			lua.L_NewLib( define );
			return 1;
		}

#if !UNITY_WEBPLAYER
		private static int OS_Clock( ILuaState lua )
		{
			lua.PushNumber( Process.GetCurrentProcess().TotalProcessorTime.TotalSeconds );
			return 1;
		}

        /// <summary>
        /// 获取从1970年1月1日到当前时间（UTC）的秒数
        /// </summary>
        /// <param name="lua"></param>
        /// <returns></returns>
        private static int OS_UtcTime(ILuaState lua)
        {
#if UNITY
            lua.PushInteger(0);
            //lua.PushInteger((int)(DateTime.UtcNow.AddSeconds(-CBasePlayerInfo.GetIntervalTime()) - DateTime.Parse("1970-1-1")).TotalSeconds);
#else
            lua.PushInteger((int)(DateTime.UtcNow - DateTime.Parse("1970-1-1")).TotalSeconds);
#endif
            return 1;
        }

        /// <summary>
        /// 获取从1970年1月1日到当前时间(本地)的秒数,UnixTimeStamp
        /// </summary>
        /// <param name="lua"></param>
        /// <returns></returns>
        private static int OS_Time(ILuaState lua)
        {
            lua.PushInteger((int)(DateTime.Now - DateTime.Parse("1970-1-1")).TotalSeconds);
            return 1;
        }

        /// <summary>
        /// String转UINT
        /// </summary>
        /// <param name="lua">lua state stack</param>
        /// <returns></returns>
        private static int STRING2UINT64(ILuaState lua)
        {
            string strNum = lua.L_CheckString(1);
            long num = 0;
            if (long.TryParse(strNum, out num))
                lua.PushUInt64((ulong)num);
            else
                lua.PushUInt64(0);
            return 1;
        }

        /// <summary>
        /// 获取日期相关
        /// </summary>
        /// <param name="lua"></param>
        /// <returns></returns>
        private static int OS_Date(ILuaState lua)
        {
            string param = lua.L_CheckString(1);
            lua.PushString(GetDate(DateTime.Now, param));
            return 1;
        }
        /// <summary>
        /// 获取日期相关(UTC)
        /// </summary>
        /// <param name="lua"></param>
        /// <returns></returns>
        private static int OS_UtcDate(ILuaState lua)
        {
            string param = lua.L_CheckString(1);
            lua.PushString(GetDate(DateTime.UtcNow, param));
            return 1;
        }
        /// <summary>
        /// 根据参数获取具体日期返回
        /// %A 星期几（英语单词）      Sunday
        /// %B 月份（英语单词）        July   
        /// %w 星期[0-6]              0  
        /// %H 小时[00-23]            12
        /// %M 分钟                   7
        /// %m 月份                   4
        /// %S 秒
        /// %d 日
        /// %x 日期短格式             04/30/15
        /// %X 时间                   11:30:59
        /// %Y 年
        /// </summary>
        /// <param name="dt"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        private static string GetDate(DateTime dt, string param)
        {
            if(string.IsNullOrEmpty(param))
                return "ERROR";

            if (param.CompareTo("%A") == 0) 
            {
                return dt.DayOfWeek.ToString();
            }
            else if (param.CompareTo("%B") == 0)
            {
                return Month[dt.Month];
            }
            else if (param.CompareTo("%w") == 0)
            {
                return string.Format(@"{0}", (int)dt.DayOfWeek);
            }
            else if (param.CompareTo("%H") == 0)
            {
                return dt.Hour.ToString();
            }
            else if (param.CompareTo("%M") == 0)
            {
                return dt.Minute.ToString();
            }
            else if (param.CompareTo("%m") == 0)
            {
               return dt.Month.ToString();
            }
            else if (param.CompareTo("%S") == 0)
            {
                return dt.Second.ToString();
            }
            else if (param.CompareTo("%d") == 0)
            {
                return dt.Day.ToString();
            }
            else if (param.CompareTo("%x") == 0)
            {
               return dt.ToShortDateString();
            }
            else if (param.CompareTo("%X") == 0)
            {
                return dt.ToLongTimeString();
            }
            else if (param.CompareTo("%Y") == 0)
            {
                return dt.Year.ToString();
            }

            return "ERROR_PARAM";
        }
#endif
        }
}

