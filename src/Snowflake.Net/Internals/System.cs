using System;

namespace Snowflake.Net.Internals;

internal static class System
{
    private static readonly Func<long> currentTimeFunc = InternalCurrentTimeMillis;

    public static long CurrentTimeMillis()
    {
        return currentTimeFunc();
    }

    private static readonly DateTime Jan1st1970 = new(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    private static long InternalCurrentTimeMillis()
    {
        return (long)(DateTime.UtcNow - Jan1st1970).TotalMilliseconds;
    }
}
