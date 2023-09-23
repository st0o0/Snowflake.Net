using System;

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
    private readonly Random _random;
    private readonly Lazy<long> _timeFunction;
    private readonly int _randomBytes;
    static readonly int NODE_BITS_256 = 8;
    static readonly int NODE_BITS_1024 = 10;
    static readonly int NODE_BITS_4096 = 12;

    static readonly Builder Instance = new();

    public SnowflakeFactory() : this(GetBuilder())
    {
    }

    public SnowflakeFactory(int node) : this(GetBuilder().WithNode(node))
    {
    }

    private SnowflakeFactory(Builder builder)
    {

        // setup the custom epoch, the node bits, etc
        _customEpoch = builder.GetCustomEpoch();
        _nodeBits = builder.GetNodeBits();
        _random = builder.GetRandom();
        _timeFunction = builder.GetTimeFunction();

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

    public static SnowflakeFactory NewInstance256()
    {
        return GetBuilder().WithNodeBits(NODE_BITS_256).Build();
    }

    public static SnowflakeFactory NewInstance256(int node)
    {
        return GetBuilder().WithNodeBits(NODE_BITS_256).WithNode(node).Build();
    }

    public static SnowflakeFactory NewInstance1024()
    {
        return GetBuilder().WithNodeBits(NODE_BITS_1024).Build();
    }

    public static SnowflakeFactory NewInstance1024(int node)
    {
        return GetBuilder().WithNodeBits(NODE_BITS_1024).WithNode(node).Build();
    }

    public static SnowflakeFactory NewInstance4096()
    {
        return GetBuilder().WithNodeBits(NODE_BITS_4096).Build();
    }

    public static SnowflakeFactory NewInstance4096(int node)
    {
        return GetBuilder().WithNodeBits(NODE_BITS_4096).WithNode(node).Build();
    }


    public SnowflakeId Create()
    {

        var _time = GetTime() << SnowflakeId.RANDOM_BITS;
        var _node = (long)this._node << _counterBits;
        var _counter = (long)this._counter & _counterMask;

        return new SnowflakeId(_time | _node | _counter);
    }

    private long GetTime()
    {
        var time = _timeFunction.Value;

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
        var bytes = new byte[_randomBytes];
        _random.NextBytes(bytes);
        return bytes.Length switch
        {
            1 => bytes[0] & 0xff & _counterMask,
            2 => (((bytes[0] & 0xff) << 8) | (bytes[1] & 0xff)) & _counterMask,
            _ => (((bytes[0] & 0xff) << 16) | ((bytes[1] & 0xff) << 8) | (bytes[2] & 0xff)) & _counterMask,
        };
    }

    public static Builder GetBuilder() => Instance;

    public class Builder
    {
        private int _node;
        private int _nodeBits;
        private long _customEpoch;
        private Random _random = new();
        private Lazy<long> _timeFunction;

        public Builder WithNode(int value)
        {
            _node = value != 0 ? value : Settings.Create().Node;
            return this;
        }

        public Builder WithNodeBits(int value)
        {
            _nodeBits = value;
            return this;
        }

        public Builder WithCustomEpoch(DateTimeOffset value)
        {
            _customEpoch = value.ToUnixTimeMilliseconds();
            return this;
        }

        public Builder WithRandom(Random value)
        {
            if (_random != null)
            {
                _random = value;
            }

            return this;
        }

        public Builder WithRandomFunction(Lazy<int> value)
        {
            _random = new Random(value.Value);
            return this;
        }


        public Builder WithDateTimeOffset(DateTimeOffset value)
        {
            _timeFunction = new Lazy<long>(() => value.ToUnixTimeMilliseconds());
            return this;
        }


        public Builder WithTimeFunction(Lazy<long> value)
        {
            _timeFunction = value;
            return this;
        }

        internal int GetNode()
        {
            if (_node < 0 || _node > int.MaxValue)
            {
                throw new IndexOutOfRangeException($"Node ID out of range [0, {int.MaxValue}]: {_node}");
            }

            return _node;
        }

        internal int GetNodeBits()
        {
            if (_nodeBits < 0 || _nodeBits > 20)
            {
                throw new InvalidOperationException($"Node bits out of range [0, 20]: {_nodeBits}");
            }

            return _nodeBits;
        }

        internal long GetCustomEpoch() => _customEpoch;

        internal Random GetRandom()
        {
            if (_random == null)
            {
                this.WithRandom(new Random());
            }
            return _random;
        }

        internal Lazy<long> GetTimeFunction()
        {
            if (_timeFunction == null)
            {
                WithTimeFunction(new Lazy<long>(() => Internals.System.CurrentTimeMillis()));
            }
            return _timeFunction;
        }

        public SnowflakeFactory Build() => new(this);
    }

    public class Settings
    {
        public static Settings Create() => new();

        public static Settings Create(int node) => new(node);

        internal Settings() { }

        internal Settings(int node)
        {
            Node = node;
        }

        public int Node { get; } = new Random().Next();
    }
}
