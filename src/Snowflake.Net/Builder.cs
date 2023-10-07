namespace Snowflake.Net;

public class Builder
{
    private int? _node;
    private int? _nodeBits;
    private long? _customEpoch;
    private IRandom _random;
    private TimeZoneInfo _timeZoneInfo;

    public Builder WithNode(int node)
    {
        _node = node;
        return this;
    }

    public Builder WithNodeBits(int nodeBits)
    {
        _nodeBits = nodeBits;
        return this;
    }

    public Builder WithCustomEpoch(DateTimeOffset customEpoch)
    {
        _customEpoch = customEpoch.ToUnixTimeMilliseconds();
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

    public Builder WithClock(TimeZoneInfo info)
    {
        _timeZoneInfo = info;
        return this;
    }

    public int GetNode()
    {
        var max = (1 << _nodeBits) - 1;

        if (!_node.HasValue)
        {
            if (EnvironmentSettings.GetNode().HasValue)
            {
                // use property or variable
                _node = EnvironmentSettings.GetNode();
            }
            else
            {
                // use random node identifier
                var value = _random.NextInt();
                _node = (int)Math.Min((uint)value, (uint)max);
            }
        }

        if (_nodeBits == 0)
            _node = 0;

        if (_node < 0 || _node > max)
        {
            throw new ArgumentException($"Node ID out of range [0, {max}]: {_node}");
        }

        return (int)_node;
    }

    /// <summary>
    ///  Get the node identifier bits length within the range 0 to 20.
    /// <returns> a number </returns>
    /// <exception cref="ArgumentException"> throws if the node bits are out of range </exception>
    /// </summary>
    public int GetNodeBits()
    {
        if (!_nodeBits.HasValue)
        {
            if (EnvironmentSettings.GetNodeCount().HasValue)
            {
                // use property or variable
                _nodeBits = (int)Math.Ceiling(Math.Log((double)EnvironmentSettings.GetNodeCount()) / Math.Log(2));
            }
            else
            {
                // use default bit length: 10 bits
                _nodeBits = SnowflakeFactory.NODE_BITS_1024;
            }
        }

        if (_nodeBits < 0 || _nodeBits > 20)
        {
            throw new ArgumentException($"Node bits out of range [0, 20]: {_nodeBits}");
        }

        return (int)_nodeBits;
    }

    /// <summary>
    /// Gets the custom epoch.
    /// <returns> a number </returns>
    /// </summary>
    public long GetCustomEpoch()
    {
        _customEpoch ??= SnowflakeId.SNOWFLAKEID_EPOCH; // 2023-01-01
        return (long)_customEpoch;
    }

    /// <summary>
    /// Gets the random generator.
    /// <returns> a random generator </returns>
    /// </summary>
    public IRandom GetRandom()
    {
        if (_random == null)
        {
            WithRandom(RandomGenerators.OfCryptographicallySecureRandom());
        }

        return _random;
    }

    /// <summary>
    /// <returns><see cref="TimeZoneInfo"/> </returns>
    /// </summary>
    public TimeZoneInfo GetTimeZoneInfo()
    {
        _timeZoneInfo ??= TimeZoneInfo.Utc;
        return _timeZoneInfo;
    }

    /// <summary>
    /// Returns a build SnowflakeId factory.
    /// <returns> a <see cref="SnowflakeFactory"/> </returns>
    /// <exception cref="ArgumentException"> throws if the node is out of range </exception>
    /// <exception cref="ArgumentException"> throws if if the node is out of range</exception>
    /// </summary>
    public SnowflakeFactory Build()
    {
        return new SnowflakeFactory(this);
    }
}