namespace Snowflake.Net;

public class Builder
{
    private int _node;
    private int _nodeBits;
    private long _customEpoch;
    private IRandom _random;
    private Lazy<long> _timeFunction;

    public Builder WithNode(int value)
    {
        _node = value != 0 ? value : EnvironmentSettings.GetNode() ?? _random.NextInt() & (1 << _nodeBits) - 1;
        return this;
    }

    public Builder WithNodeBits(int value)
    {
        _nodeBits = value != 0 ? value : EnvironmentSettings.GetNodeCount() ?? 10;
        return this;
    }

    public Builder WithCustomEpoch(DateTimeOffset value)
    {
        _customEpoch = value.ToUnixTimeMilliseconds();
        return this;
    }

    public Builder WithRandom(RandomGenerators randoms)
    {
        if (randoms != null)
        {
            if (randoms.IsCryptographicallySecure())
            {
                _random = new ByteRandom(randoms);
            }
            else
            {
                _random = new IntRandom(randoms);
            }
        }

        return this;
    }

    public Builder WithRandom(IRandom value)
    {
        if (_random != null)
        {
            _random = value;
        }

        return this;
    }

    public Builder WithRandomFunction(Func<int> randomFunction)
    {
        _random = new IntRandom(randomFunction);
        return this;
    }

    public Builder WithRandomFunction(Func<int, byte[]> randomFunction)
    {
        _random = new ByteRandom(randomFunction);
        return this;
    }

    public Builder WithDateTimeOffset(DateTimeOffset value)
    {
        _timeFunction = new Lazy<long>(value.ToUnixTimeMilliseconds);
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

    internal IRandom GetRandom()
    {
        if (_random == null)
        {
            WithRandom(RandomGenerators.OfCryptographicallySecureRandom());
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

    public SnowflakeFactory Build() => new SnowflakeFactory(this);
}