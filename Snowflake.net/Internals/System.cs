using System;
using System.Collections.Generic;
using System.Text;

namespace Snowflake.net.Internals
{
    internal static class System
    {
        public static Func<long> currentTimeFun = InternalCurrentTimeMillis;



        private static readonly DateTime Jan1st1970 = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        private static long InternalCurrentTimeMillis() 
        {
            return (long)(DateTime.UtcNow - Jan1st1970).TotalMilliseconds;
        }
    }
}
