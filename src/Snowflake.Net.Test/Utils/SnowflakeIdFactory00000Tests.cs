namespace Snowflake.Net.Test;

public class SnowflakeIdFactory00000Tests
{
    private const int SnowflakeIdLength = 13;

    protected const int LoopMax = 10000;

    protected static readonly RandomGenerators Random = RandomGenerators.OfSimpleRandomNumberGenerator();

    protected static readonly int MultiplePasses = AvailableProcessors();

    protected static bool CheckNullOrInvalid(long[] list)
    {
        foreach (var item in list)
        {
            item.Should().NotBe(0);
        }
        return true; // success
    }

    protected static bool CheckNullOrInvalid(string[] list)
    {
        foreach (var item in list)
        {
            item.Should().NotBeNull();
            item.Should().NotBeEmpty();
            item.Should().NotBeNullOrWhiteSpace();
            item.Length.Should().Be(SnowflakeIdLength);
            SnowflakeId.IsValid(item).Should().BeTrue();
        }
        return true; // success
    }

    protected static bool CheckUniqueness(long[] list)
    {

        var set = new HashSet<long>();

        foreach (var item in list)
        {
            set.Add(item).Should().BeTrue();
        }

        set.Count.Should().Be(list.Length);
        return true; // success
    }

    protected static bool CheckUniqueness(string[] list)
    {

        var set = new HashSet<string>();

        foreach (var item in list)
        {
            set.Add(item).Should().BeTrue();
        }

        set.Count.Should().Be(list.Length);
        return true; // success
    }

    protected static bool CheckOrdering(long[] list)
    {
        // copy the list
        var other = new long[list.Length];
        Array.Copy(list, 0, other, 0, list.Length);
        Array.Sort(other);

        for (var i = 0; i < list.Length; i++)
        {
            list[i].Should().Be(other[i]);
        }
        return true; // success
    }

    protected static bool CheckOrdering(string[] list)
    {
        var other = new string[list.Length];
        Array.Copy(list, 0, other, 0, list.Length);
        Array.Sort(other);

        for (var i = 0; i < list.Length; i++)
        {
            list[i].Should().Be(other[i]);
        }
        return true; // success
    }

    protected static bool CheckMaximumPerMs(long[] list, int max)
    {
        var dict = new Dictionary<long, List<long>>();

        foreach (var item in list)
        {
            var key = SnowflakeId.From(item).GetTime;
            if (!dict.ContainsKey(key))
            {
                dict.Add(key, new List<long>());
            }

            dict[key].Add(item);
            var size = dict[key].Count;


            var notTooManySnowflakeIdsPerMillisecond = size <= max;
            notTooManySnowflakeIdsPerMillisecond.Should().BeTrue();
        }

        return true; // success
    }

    protected bool CheckMaximumPerMs(string[] list, int max)
    {
        var dict = new Dictionary<long, List<string>>();

        foreach (var item in list)
        {
            var key = SnowflakeId.From(item).GetTime;
            if (!dict.ContainsKey(key))
            {
                dict.Add(key, new List<string>());
            }

            dict[key].Add(item);
            var size = dict[key].Count;


            var notTooManySnowflakeIdsPerMillisecond = size <= max;
            notTooManySnowflakeIdsPerMillisecond.Should().BeTrue();
        }

        return true; // success
    }

    protected static bool CheckCreationTime(long[] list, long startTime, long endTime)
    {

        (startTime <= endTime).Should().BeTrue();

        foreach (var item in list)
        {
            var creationTime = SnowflakeId.From(item).GetUnixMilliseconds();
            (creationTime >= startTime).Should().BeTrue();
            (creationTime <= endTime + LoopMax).Should().BeTrue();
        }
        return true; // success
    }

    protected static bool CheckCreationTime(string[] list, long startTime, long endTime)
    {

        (startTime <= endTime).Should().BeTrue();

        foreach (var item in list)
        {
            var creationTime = SnowflakeId.From(item).GetUnixMilliseconds();
            (creationTime >= startTime).Should().BeTrue();
            (creationTime <= endTime + LoopMax).Should().BeTrue();
        }
        return true; // success
    }

    private static int AvailableProcessors()
    {
        var processors = Environment.ProcessorCount;
        if (processors < 4)
        {
            processors = 4;
        }
        return processors;
    }
}
