using Snowflake.Net.Extensions;

namespace Snowflake.Net.Internals;

internal class StaticRandom : Random
{
    public static StaticRandom Instance { get; } = new StaticRandom();

    private static int _seed = Environment.TickCount;

    private static readonly ThreadLocal<Random> Random = new ThreadLocal<Random>(() => new Random(Interlocked.Increment(ref _seed)));

    public override int Next() => Random.Value.Next();

    public override void NextBytes(byte[] bytes) => Random.Value.NextBytes(bytes);

    public long NextLong(long? min = default, long? max = default)
    {
        return min switch
        {
            null when max == default => Random.Value.NextLong(long.MinValue, long.MaxValue),
            null => Random.Value.NextLong(long.MinValue, long.MaxValue),
            _ => Random.Value.NextLong((long)min, (long)max)
        };
    }
}
