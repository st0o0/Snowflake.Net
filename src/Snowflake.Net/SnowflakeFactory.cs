using System;

namespace Snowflake.Net;

public class SnowflakeFactory
{
    private int _counter;
    private long _lastTime;
    private int _node;
    private int _nodeBits;
    private int _counterBits;
    private int _nodeMask;
    private int _counterMask;
    private long _customEpoch;
    private Random _random;
    private Lazy<long> _timeFunction;
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
        return SnowflakeFactory.GetBuilder().WithNodeBits(NODE_BITS_256).Build();
    }

    public static SnowflakeFactory NewInstance256(int node)
    {
        return SnowflakeFactory.GetBuilder().WithNodeBits(NODE_BITS_256).WithNode(node).Build();
    }

    public static SnowflakeFactory NewInstance1024()
    {
        return SnowflakeFactory.GetBuilder().WithNodeBits(NODE_BITS_1024).Build();
    }

    public static SnowflakeFactory NewInstance1024(int node)
    {
        return SnowflakeFactory.GetBuilder().WithNodeBits(NODE_BITS_1024).WithNode(node).Build();
    }

    public static SnowflakeFactory NewInstance4096()
    {
        return SnowflakeFactory.GetBuilder().WithNodeBits(NODE_BITS_4096).Build();
    }

    public static SnowflakeFactory NewInstance4096(int node)
    {
        return SnowflakeFactory.GetBuilder().WithNodeBits(NODE_BITS_4096).WithNode(node).Build();
    }


    public SnowflakeId Create()
    {

        long _time = GetTime() << SnowflakeId.RANDOM_BITS;
        long _node = (long)this._node << _counterBits;

        long _counter = (long)this._counter & _counterMask;

        return new SnowflakeId(_time | _node | _counter);
    }

    private long GetTime()
    {

        long time = _timeFunction.Value;

        if (time <= this._lastTime)
        {
            _counter++;
            // Carry is 1 if an overflow occurs after ++.
            int carry = _counter >>> _counterBits;
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
        byte[] bytes = new byte[_randomBytes];
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
        private int node;
        private int nodeBits;
        private long customEpoch;
        private Random random = new();
        private Lazy<long> timeFunction;

        public Builder WithNode(int value)
        {
            node = value != 0 ? value : Settings.Create().Node;
            return this;
        }

        public Builder WithNodeBits(int value)
        {
            nodeBits = value;
            return this;
        }

        public Builder WithCustomEpoch(DateTimeOffset value)
        {
            customEpoch = value.ToUnixTimeMilliseconds();
            return this;
        }

        public Builder WithRandom(Random value)
        {
            if (random != null)
            {
                random = value;
            }
            return this;
        }

        public Builder WithRandomFunction(Lazy<int> value)
        {
            random = new Random(value.Value);
            return this;
        }


        public Builder WithDateTimeOffset(DateTimeOffset value)
        {
            timeFunction = new Lazy<long>(() => value.ToUnixTimeMilliseconds());
            return this;
        }


        public Builder WithTimeFunction(Lazy<long> value)
        {
            timeFunction = value;
            return this;
        }

        internal int GetNode()
        {
            if (node < 0 || node > int.MaxValue)
            {
                throw new IndexOutOfRangeException(string.Format("Node ID out of range [0, %s]: %s", int.MaxValue, node));
            }

            return node;
        }

        internal int GetNodeBits()
        {   
            if (nodeBits < 0 || nodeBits > 20)
            {
                throw new InvalidOperationException(string.Format("Node bits out of range [0, 20]: %s", nodeBits));
            }

            return nodeBits;
        }

        internal long GetCustomEpoch() => customEpoch;

        internal Random GetRandom()
        {
            if (random == null)
            {
                this.WithRandom(new Random());
            }
            return random;
        }

        internal Lazy<long> GetTimeFunction()
        {
            if (timeFunction == null)
            {
                WithTimeFunction(new Lazy<long>(() => Internals.System.CurrentTimeMillis()));
            }
            return timeFunction;
        }

        public SnowflakeFactory Build() => new(this);
    }

    public class Settings
    {
        public static Settings Create() => new();

        public static Settings Create(int node, int nodeCount) => new(node);

        internal Settings() { }

        internal Settings(int node)
        {
            Node = node;
        }

        public int Node { get; } = new Random().Next();
    }
}
