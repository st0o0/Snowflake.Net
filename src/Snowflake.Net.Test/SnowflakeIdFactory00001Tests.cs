using System.Collections.Concurrent;
using FluentAssertions.Common;

namespace Snowflake.Net.Test;

public class SnowflakeIdFactory00001Tests : SnowflakeIdFactory00000Tests
{
    private const int NodeBits = 0;
    private const int CounterBits = 22;

    private static readonly int NodeMax = (int)Math.Pow(2, NodeBits);
    private static readonly int CounterMax = (int)Math.Pow(2, CounterBits);

    [Fact]
    public void TestGetSnowflakeId1()
    {
        var startTime = DateTime.UtcNow.ToDateTimeOffset().ToUnixTimeMilliseconds();
        var factory = SnowflakeFactory.GetBuilder().WithNodeBits(NodeBits).WithRandom(Random).Build();

        var list = new long[LoopMax];
        for (var i = 0; i < LoopMax; i++)
        {
            list[i] = factory.Create().ToLong();
        }

        var endTime = DateTime.UtcNow.ToDateTimeOffset().ToUnixTimeMilliseconds();

        CheckNullOrInvalid(list).Should().BeTrue();
        CheckUniqueness(list).Should().BeTrue();
        CheckOrdering(list).Should().BeTrue();
        CheckMaximumPerMs(list, CounterMax).Should().BeTrue();
        CheckCreationTime(list, startTime, endTime).Should().BeTrue();
    }

    [Fact]
    public void TestGetSnowflakeId1WithNode()
    {
        var startTime = DateTime.UtcNow.ToDateTimeOffset().ToUnixTimeMilliseconds();
        var node = Math.Abs(Random.NextInt(NodeMax));
        var factory = SnowflakeFactory.GetBuilder().WithNode(node).WithNodeBits(NodeBits).WithRandom(Random).Build();

        var list = new long[LoopMax];
        for (var i = 0; i < LoopMax; i++)
        {
            list[i] = factory.Create().ToLong();
        }

        var endTime = DateTime.UtcNow.ToDateTimeOffset().ToUnixTimeMilliseconds();

        CheckNullOrInvalid(list).Should().BeTrue();
        CheckUniqueness(list).Should().BeTrue();
        CheckOrdering(list).Should().BeTrue();
        CheckMaximumPerMs(list, CounterMax).Should().BeTrue();
        CheckCreationTime(list, startTime, endTime).Should().BeTrue();
    }

    [Fact]
    public void TestGetSnowflakeIdString1()
    {
        var startTime = DateTime.UtcNow.ToDateTimeOffset().ToUnixTimeMilliseconds();
        var factory = SnowflakeFactory.GetBuilder().WithNodeBits(NodeBits).WithRandom(Random).Build();

        var list = new string[LoopMax];
        for (var i = 0; i < LoopMax; i++)
        {
            list[i] = factory.Create().ToString();
        }

        var endTime = DateTime.UtcNow.ToDateTimeOffset().ToUnixTimeMilliseconds();

        CheckNullOrInvalid(list).Should().BeTrue();
        CheckUniqueness(list).Should().BeTrue();
        CheckOrdering(list).Should().BeTrue();
        CheckMaximumPerMs(list, CounterMax).Should().BeTrue();
        CheckCreationTime(list, startTime, endTime).Should().BeTrue();
    }

    [Fact]
    public void TestGetSnowflakeIdString1WithNode()
    {

        var startTime = DateTime.UtcNow.ToDateTimeOffset().ToUnixTimeMilliseconds();
        var node = Math.Abs(Random.NextInt(NodeMax));
        var factory = SnowflakeFactory.GetBuilder().WithNode(node).WithNodeBits(NodeBits).WithRandom(Random).Build();

        var list = new string[LoopMax];
        for (var i = 0; i < LoopMax; i++)
        {
            list[i] = factory.Create().ToString();
        }

        var endTime = DateTime.UtcNow.ToDateTimeOffset().ToUnixTimeMilliseconds();

        CheckNullOrInvalid(list).Should().BeTrue();
        CheckUniqueness(list).Should().BeTrue();
        CheckOrdering(list).Should().BeTrue();
        CheckMaximumPerMs(list, CounterMax).Should().BeTrue();
        CheckCreationTime(list, startTime, endTime).Should().BeTrue();
    }

    [Fact]
    public void TestGetSnowflakeId1Parallel()
    {
        var sets = new ConcurrentDictionary<string, byte>[MultiplePasses];
        var counterMax = CounterMax / MultiplePasses;

        // Instantiate and start many threads
        for (var i = 0; i < MultiplePasses; i++)
        {
            var factory = SnowflakeFactory.GetBuilder().WithNodeBits(NodeBits).WithRandom(Random).Build();
            sets[i] = new ConcurrentDictionary<string, byte>();
            Parallel.For(0, counterMax, j =>
            {
                sets[i].TryAdd(factory.Create().ToString(), 0);
            });
        }

        // Check if the quantity of unique UUIDs is correct
        var sum = sets.Select(x => x.Count).Aggregate((a, b) => a + b);
        (counterMax * MultiplePasses).Should().Be(sum);
    }
}
