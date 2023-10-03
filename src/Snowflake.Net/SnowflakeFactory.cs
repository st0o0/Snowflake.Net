namespace Snowflake.Net;

public class SnowflakeFactory
{
    private int _counter;
    private long _lastTime;
    private readonly int _node;
    private readonly int _nodeBits;
    private readonly int _counterBits;
    private readonly int _nodeMask;
    private readonly int _counterMask;
    private readonly long _customEpoch;
    private readonly IRandom _random;
    private readonly TimeZoneInfo _timeZoneInfo;
    private readonly int _randomBytes;
    internal static readonly int NODE_BITS_256 = 8;
    internal static readonly int NODE_BITS_1024 = 10;
    internal static readonly int NODE_BITS_4096 = 12;

    private readonly object _lock = new();
    private readonly object _lock2 = new();

    static readonly Builder Instance = new();

    public SnowflakeFactory() : this(GetBuilder())
    {
    }

    public SnowflakeFactory(int node) : this(GetBuilder().WithNode(node))
    {
    }

    internal SnowflakeFactory(Builder builder)
    {

        // setup the custom epoch, the node bits, etc
        _customEpoch = builder.GetCustomEpoch();
        _nodeBits = builder.GetNodeBits();
        _random = builder.GetRandom();
        _timeZoneInfo = builder.GetTimeZoneInfo();

        // setup constants that depend on node bits
        _counterBits = SnowflakeId.RANDOM_BITS - _nodeBits;
        _counterMask = SnowflakeId.RANDOM_MASK >>> _nodeBits;
        _nodeMask = SnowflakeId.RANDOM_MASK >>> _counterBits;

        // setup how many bytes to get from the random function
        _randomBytes = ((_counterBits - 1) / 8) + 1;

        // setup the node identifier
        _node = builder.GetNode() & _nodeMask;

        // finally initialize inner state
        _lastTime = 0L; // 1970-01-01
        _counter = GetRandomCounter();
    }

    public static SnowflakeFactory NewInstance256() => GetBuilder().WithNodeBits(NODE_BITS_256).Build();

    public static SnowflakeFactory NewInstance256(int node) => GetBuilder().WithNodeBits(NODE_BITS_256).WithNode(node).Build();

    public static SnowflakeFactory NewInstance1024() => GetBuilder().WithNodeBits(NODE_BITS_1024).Build();

    public static SnowflakeFactory NewInstance1024(int node) => GetBuilder().WithNodeBits(NODE_BITS_1024).WithNode(node).Build();

    public static SnowflakeFactory NewInstance4096() => GetBuilder().WithNodeBits(NODE_BITS_4096).Build();

    public static SnowflakeFactory NewInstance4096(int node) => GetBuilder().WithNodeBits(NODE_BITS_4096).WithNode(node).Build();

    public SnowflakeId Create()
    {
        lock (_lock)
        {
            var time = GetTime() << SnowflakeId.RANDOM_BITS;
            var node = (long)_node << _counterBits;
            var counter = (long)_counter & _counterMask;

            var number = time | node | counter;
            // Debug.Assert(number >= 0, "number is negative: " + number);
            return new SnowflakeId(number);
        }
    }

    private long GetTime()
    {
        var time = DateTimeOffset.UtcNow.ToOffset(_timeZoneInfo.BaseUtcOffset).ToUnixTimeMilliseconds();

        if (time <= _lastTime)
        {
            _counter++;
            // Carry is 1 if an overflow occurs after ++.
            var carry = _counter >>> _counterBits;
            _counter &= _counterMask;
            time = _lastTime + carry; // increment time
        }
        else
        {
            // If the system clock has advanced as expected,
            // simply reset the counter to a new random value.
            _counter = GetRandomCounter();
        }

        // save current time
        _lastTime = time;

        // adjust to the custom epoch
        return time - _customEpoch;
    }

    private int GetRandomCounter()
    {
        lock (_lock2)
        {
            if (_random is ByteRandom)
            {
                var bytes = _random.NextBytes(_randomBytes);

                return bytes.Length switch
                {
                    1 => bytes[0] & 0xff & _counterMask,
                    2 => (((bytes[0] & 0xff) << 8) | (bytes[1] & 0xff)) & _counterMask,
                    _ => (((bytes[0] & 0xff) << 16) | ((bytes[1] & 0xff) << 8) | (bytes[2] & 0xff)) & _counterMask,
                };
            }
            else
            {
                return _random.NextInt() & _counterMask;
            }
        }
    }

    public static Builder GetBuilder() => Instance;
}