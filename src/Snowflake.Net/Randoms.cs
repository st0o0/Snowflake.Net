namespace Snowflake.Net;

public interface IRandom
{
    public int NextInt();

    public byte[] NextBytes(int length);
}

public class IntRandom : IRandom
{
    private readonly Func<int> _randomFunction;

    public IntRandom() : this((Func<int>)null)
    {
    }

    public IntRandom(RandomGenerators random) : this(NewRandomFunction(random))
    {
    }

    public IntRandom(Func<int> randomFunction)
    {
        _randomFunction = randomFunction ?? NewRandomFunction(null);
    }

    public int NextInt()
    {
        return _randomFunction();
    }

    public byte[] NextBytes(int length)
    {
        var shift = 0;
        var random = 0;
        var bytes = new byte[length];

        for (var i = 0; i < length; i++)
        {
            if (shift < 8) // Byte.SIZE
            {
                shift = 32; // Integer.SIZE
                random = _randomFunction();
            }

            shift -= 8; // Byte.SIZE
            bytes[i] = (byte)(random >> shift);
        }

        return bytes;
    }

    private static Func<int> NewRandomFunction(RandomGenerators randoms)
    {
        var entropy = randoms ?? RandomGenerators.OfCryptographicallySecureRandom();
        return () => entropy.NextInt();
    }
}

public class ByteRandom : IRandom
{
    private readonly Func<int, byte[]> _randomFunction;

    public ByteRandom() : this(NewRandomFunction(null))
    {
    }

    public ByteRandom(RandomGenerators randoms) : this(NewRandomFunction(randoms))
    {
    }

    public ByteRandom(Func<int, byte[]> randomFunction)
    {
        _randomFunction = randomFunction ?? NewRandomFunction(null);
    }

    public int NextInt()
    {
        var number = 0;
        var bytes = _randomFunction.Invoke(4);
        for (var i = 0; i < 4; i++) // Integer.SIZE
        {
            number = (number << 8) | (bytes[i] & 0xff);
        }

        return number;
    }

    public byte[] NextBytes(int length)
    {
        return _randomFunction.Invoke(length);
    }

    private static Func<int, byte[]> NewRandomFunction(RandomGenerators randoms)
    {
        var entropy = randoms ?? RandomGenerators.OfCryptographicallySecureRandom();
        return length => entropy.NextBytes(length);
    }
}
