namespace Snowflake.Net.Internals;

internal static class LazyHolder
{
    private static int _counter = StaticRandom.Instance.Next();

    public static int IncrementAndGet()
    {
        return Interlocked.Increment(ref _counter);
    }
}
