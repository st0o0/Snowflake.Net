using Snowflake.Net.Internals;
using System.Collections.Concurrent;

namespace Snowflake.Net.Test;

public class CollisionTests
{
    private static SnowflakeFactory NewFactory(int nodeBits)
    {
        return SnowflakeFactory.GetBuilder().WithRandomFunction(StaticRandom.Instance.Next)
            .WithNodeBits(nodeBits) // 8 bits: 256 nodes; 10 bits: 1024 nodes...
            .Build();
    }

    [Fact]
    public void TestCollision()
    {
        var nodeBits = 8;
        var threadCount = 16;
        var iterationCount = 100_000;

        var clashes = 0;
        var endLatch = new CountDownLatch(threadCount);
        var snowflakeIdMap = new ConcurrentDictionary<long, int>();

        // one generator shared by ALL THREADS
        var factory = NewFactory(nodeBits);

        for (var i = 0; i < threadCount; i++)
        {
            var threadId = i;

            new Thread(() =>
            {
                for (var j = 0; j < iterationCount; j++)
                {
                    var snowflakeId = factory.Create().ToLong();
                    if (snowflakeIdMap.ContainsKey(snowflakeId))
                    {
                        Interlocked.Increment(ref clashes);
                        break;
                    }
                    var success = snowflakeIdMap.TryAdd(
                        snowflakeId,
                        threadId * iterationCount + j
                    );
                    if (!success)
                    {
                        Interlocked.Increment(ref clashes);
                        break;
                    }
                }

                endLatch.countDown();
            }).Start();
        }

        endLatch.await();

        // assertFalse("Collisions detected!", clashes.intValue() != 0);
        clashes.Should().Be(0);
    }
}
