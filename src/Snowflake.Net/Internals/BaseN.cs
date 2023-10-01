using System.Numerics;
using Snowflake.Net.Extensions;

namespace Snowflake.Net;

internal static class BaseN
{
    static readonly BigInteger MAX = BigInteger.Subtract(BigInteger.Pow(new BigInteger(2), 64), BigInteger.One);
    static readonly string ALPHABET = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz"; // base-62

    internal static string Encode(SnowflakeId snowflakeId, int baseN)
    {
        const int longSize = 64;
        var x = snowflakeId.ToBytes().ToUnsignedBigEndianBigInteger();
        var radix = BigInteger.Parse(baseN.ToString());
        var length = (int)Math.Ceiling(longSize / (Math.Log(baseN) / Math.Log(2)));
        var b = length;
        var buffer = new char[length];
        while (x.CompareTo(BigInteger.Zero) > 0)
        {
            var quotient = BigInteger.DivRem(x, radix, out var remainder);
            buffer[--b] = ALPHABET[(int)(uint)(remainder & uint.MaxValue)];
            x = quotient;
        }

        while (b > 0)
        {
            buffer[--b] = '0';
        }

        return new string(buffer);
    }

    internal static SnowflakeId Decode(string value, int baseN)
    {

        var x = BigInteger.Zero;
        var length = (int)Math.Ceiling(64 / (Math.Log(baseN) / Math.Log(2)));
        if (value == null)
        {
            throw new ArgumentException($"Invalid base-{baseN} string: null");
        }

        if (value.Length != length)
        {
            throw new ArgumentException($"Invalid base-{baseN} length: {value.Length}");
        }

        for (var i = 0; i < value.Length; i++)
        {
            long plus = ALPHABET.IndexOf(value[i]);
            if (plus < 0 || plus >= baseN)
            {
                throw new ArgumentException($"Invalid base-{baseN} character: {value[i]}");
            }

            x = x * baseN + new BigInteger(plus);
        }

        if (x.CompareTo(MAX) > 0)
        {
            throw new ArgumentException($"Invalid base-{baseN} value (overflow): {x}");
        }

        var number = (long)(ulong)(x & ulong.MaxValue);

        return new SnowflakeId(number);
    }
}
