using System.Numerics;
using Snowflake.Net.Extensions;

namespace Snowflake.Net;

internal static class BaseN
{
    private static readonly BigInteger Max = BigInteger.Parse("18446744073709551616");
    private const string Alphabet = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz"; // base-62

    public static string Encode(SnowflakeId tsid, int @base)
    {
        const int longSize = 64;
        var x = tsid.ToBytes().ToUnsignedBigEndianBigInteger();
        var radix = BigInteger.Parse(@base.ToString());
        var length = (int)Math.Ceiling(longSize / (Math.Log(@base) / Math.Log(2)));
        var b = length;
        var buffer = new char[length];
        while (x.CompareTo(BigInteger.Zero) > 0)
        {
            var quotient = BigInteger.DivRem(x, radix, out var remainder);
            buffer[--b] = Alphabet[(int)(uint)(remainder & uint.MaxValue)];
            x = quotient;
        }
        while (b > 0)
            buffer[--b] = '0';
        return new string(buffer);
    }

    public static SnowflakeId Decode(string str, int @base)
    {
        var x = BigInteger.Zero;
        var length = (int)Math.Ceiling(64 / (Math.Log(@base) / Math.Log(2)));
        if (str == null)
        {
            throw new ArgumentException($"Invalid base-{@base} string: null");
        }
        if (str.Length != length)
        {
            throw new ArgumentException($"Invalid base-{@base} length: {str.Length}");
        }
        for (var i = 0; i < str.Length; i++)
        {
            long plus = Alphabet.IndexOf(str[i]);
            if (plus < 0 || plus >= @base)
            {
                throw new ArgumentException($"Invalid base-{@base} character: {str[i]}");
            }

            // x = BigInteger.Add(BigInteger.Multiply(x,radix),BigInteger.Parse(plus.ToString()));
            x = x * @base + new BigInteger(plus);
        }
        if (x.CompareTo(Max) > 0)
        {
            throw new ArgumentException($"Invalid base-{@base} value (overflow): {x}");
        }

        var number = (long)(ulong)(x & ulong.MaxValue);

        return new SnowflakeId(number);
    }
}