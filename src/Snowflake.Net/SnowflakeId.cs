using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Text.RegularExpressions;
using DotNext.Threading;

namespace Snowflake.Net;

[Serializable]
public sealed partial class SnowflakeId : EqualityComparer<SnowflakeId>
{
    private readonly long _number = 0L;
    private static int _atomic = new Random().Next();
    internal static readonly int TSID_BYTES = 8;
    internal static readonly int TSID_CHARS = 13;
    internal static readonly long TSID_EPOCH = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).Millisecond;
    internal static readonly int RANDOM_BITS = 22;
    internal static readonly int RANDOM_MASK = 0x003fffff;

    private static readonly char[] ALPHABET_UPPERCASE = //
			{ '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', //
					'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'J', 'K', //
					'M', 'N', 'P', 'Q', 'R', 'S', 'T', 'V', 'W', 'X', 'Y', 'Z' };

    private static readonly char[] ALPHABET_LOWERCASE = //
            { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', //
					'a', 'b', 'c', 'd', 'e', 'f', 'g', 'h', 'j', 'k', //
					'm', 'n', 'p', 'q', 'r', 's', 't', 'v', 'w', 'x', 'y', 'z' };

    private static readonly Dictionary<char, byte> ALPHABET_VALUES = new()
    {
        // numbers
        { '0', 0x00 },
        { '1', 0x01 },
        { '2', 0x02 },
        { '3', 0x03 },
        { '4', 0x04 },
        { '5', 0x05 },
        { '6', 0x06 },
        { '7', 0x07 },
        { '8', 0x08 },
        { '9', 0x09 },
        // Lower case
        { 'a', 0x0a },
        { 'b', 0x0b },
        { 'c', 0x0c },
        { 'd', 0x0d },
        { 'e', 0x0e },
        { 'f', 0x0f },
        { 'g', 0x10 },
        { 'h', 0x11 },
        { 'j', 0x12 },
        { 'k', 0x13 },
        { 'm', 0x14 },
        { 'n', 0x15 },
        { 'p', 0x16 },
        { 'q', 0x17 },
        { 'r', 0x18 },
        { 's', 0x19 },
        { 't', 0x1a },
        { 'v', 0x1b },
        { 'w', 0x1c },
        { 'x', 0x1d },
        { 'y', 0x1e },
        { 'z', 0x1f },
        // Lower case OIL
        { 'o', 0x00 },
        { 'i', 0x01 },
        { 'l', 0x01 },
        // Upper case
        { 'A', 0x0a },
        { 'B', 0x0b },
        { 'C', 0x0c },
        { 'D', 0x0d },
        { 'E', 0x0e },
        { 'F', 0x0f },
        { 'G', 0x10 },
        { 'H', 0x11 },
        { 'J', 0x12 },
        { 'K', 0x13 },
        { 'M', 0x14 },
        { 'N', 0x15 },
        { 'P', 0x16 },
        { 'Q', 0x17 },
        { 'R', 0x18 },
        { 'S', 0x19 },
        { 'T', 0x1a },
        { 'V', 0x1b },
        { 'W', 0x1c },
        { 'X', 0x1d },
        { 'Y', 0x1e },
        { 'Z', 0x1f },
        // Upper case OIL
        { 'O', 0x00 },
        { 'I', 0x01 },
        { 'L', 0x01 },
    };

    public SnowflakeId(long number)
    {
        _number = number;
    }

    public static SnowflakeId From(long number) => new(number);

    public static SnowflakeId From(byte[] bytes)
    {
        if (bytes == null || bytes.Length != TSID_BYTES)
        {
            throw new InvalidOperationException("Invalid TSID bytes"); // null or wrong length!
        }

        var number = 0L;

        number |= (bytes[0x0] & 0xffL) << 56;
        number |= (bytes[0x1] & 0xffL) << 48;
        number |= (bytes[0x2] & 0xffL) << 40;
        number |= (bytes[0x3] & 0xffL) << 32;
        number |= (bytes[0x4] & 0xffL) << 24;
        number |= (bytes[0x5] & 0xffL) << 16;
        number |= (bytes[0x6] & 0xffL) << 8;
        number |= bytes[0x7] & 0xffL;

        return From(number);
    }

    public static SnowflakeId From(string value) => From(Base32.Decode(value));

    public static SnowflakeId Decode(string value, int number) => BaseN.Decode(value, number);

    public static SnowflakeId Fast()
    {
        var time = Internals.System.CurrentTimeMillis() - TSID_EPOCH << RANDOM_BITS;
        var tail = (long)_atomic.IncrementAndGet() & RANDOM_MASK;
        return new SnowflakeId(time | tail);
    }

    public long ToLong() => _number;

    public byte[] ToBytes()
    {
        var result = new byte[TSID_BYTES];

        result[0x0] = (byte)(_number >>> 56);
        result[0x1] = (byte)(_number >>> 48);
        result[0x2] = (byte)(_number >>> 40);
        result[0x3] = (byte)(_number >>> 32);
        result[0x4] = (byte)(_number >>> 24);
        result[0x5] = (byte)(_number >>> 16);
        result[0x6] = (byte)(_number >>> 8);
        result[0x7] = (byte)_number;

        return result;
    }

    public string ToLower() => ToString(ALPHABET_LOWERCASE, _number);

    public long GetUnixMilliseconds() => GetTime() + TSID_EPOCH;

    public long GetUnixMilliseconds(long customEpoch) => GetTime() + customEpoch;

    public string Encode(int value) => BaseN.Encode(this, value);

    public string Format(string format)
    {
        if (format != null)
        {
            var i = format.IndexOf("%");
            if (i < 0 || i == format.Length - 1)
            {
                throw new InvalidOperationException($"Invalid format string: {format}");
            }

            var head = format[..i];
            var tail = format[(i + 2)..];
            var placeholder = format.ElementAt(i + 1);
            return placeholder switch
            {
                // canonical string in upper case
                'S' => head + ToString() + tail,
                // canonical string in lower case
                's' => head + ToLower() + tail,
                // hexadecimal in upper case
                'X' => head + BaseN.Encode(this, 16) + tail,
                // hexadecimal in lower case
                'x' => head + BaseN.Encode(this, 16).ToLower() + tail,
                // base-10
                'd' => head + BaseN.Encode(this, 10) + tail,
                // base-62
                'z' => head + BaseN.Encode(this, 62) + tail,
                _ => throw new InvalidOperationException($"Invalid placeholder: {placeholder}"),
            };
        }

        throw new InvalidOperationException($"Invalid format string: {format}");
    }

    public static SnowflakeId Unformat(string formatted, string format)
    {
        if (formatted != null && format != null)
        {
            var i = format.IndexOf("%");
            if (i < 0 || i == format.Length - 1)
            {
                throw new InvalidOperationException($"Invalid format string: {format}");
            }

            var head = format[..i];
            var tail = format[(i + 2)..];
            var placeholder = format.ElementAt(i + 1);
            var length = formatted.Length - head.Length - tail.Length;
            if (formatted.StartsWith(head) && formatted.EndsWith(tail))
            {
                return placeholder switch
                {
                    // canonical string (case insensitive)
                    'S' => From(formatted.Substring(i, i + length)),
                    // canonical string (case insensitive)
                    's' => From(formatted.Substring(i, i + length)),
                    // hexadecimal (case insensitive)
                    'X' => BaseN.Decode(formatted.Substring(i, i + length).ToUpper(), 16),
                    // hexadecimal (case insensitive)
                    'x' => BaseN.Decode(formatted.Substring(i, i + length).ToUpper(), 16),
                    // base-10
                    'd' => BaseN.Decode(formatted.Substring(i, i + length), 10),
                    // base-62
                    'z' => BaseN.Decode(formatted.Substring(i, i + length), 62),
                    _ => throw new InvalidOperationException($"Invalid placeholder: {placeholder}"),
                };
            }
        }
        throw new InvalidOperationException($"Invalid formatted string: {formatted}");
    }

    public override bool Equals(SnowflakeId value, SnowflakeId other)
    {
        if (other == null || value == null)
            return false;
        if (other.GetType() != typeof(SnowflakeId) || value.GetType() != typeof(SnowflakeId))
            return false;
        return value._number == other._number;
    }

    public override string ToString() => Base32.Encode(ToBytes());

    public override int GetHashCode(SnowflakeId obj)
    {
        return (int)(_number ^ (_number >>> 32));
    }

    static char[] ToCharArray(string value)
    {
        var chars = value.ToCharArray();
        if (!IsValidCharArray(chars))
        {
            throw new InvalidOperationException($"Invalid TSID string: {value}");
        }

        return chars;
    }

    static bool IsValidCharArray(char[] chars)
    {

        if (chars == null || chars.Length != TSID_CHARS)
        {
            return false; // null or wrong size!
        }

        // The extra bit added by base-32 encoding must be zero
        // As a consequence, the 1st char of the input string must be between 0 and F.
        if ((ALPHABET_VALUES[chars[0]] & 0b10000) != 0)
        {
            return false; // overflow!
        }

        for (var i = 0; i < chars.Length; i++)
        {
            if (!ALPHABET_VALUES.ContainsKey(chars[i]))
            {
                return false; // invalid character!
            }
        }
        return true; // It seems to be OK.
    }

    static string ToString(char[] alphabet, long number)
    {
        var chars = new char[TSID_CHARS];

        chars[0x00] = alphabet[(char)(number >>> 60) & 0b11111];
        chars[0x01] = alphabet[(char)(number >>> 55) & 0b11111];
        chars[0x02] = alphabet[(char)(number >>> 50) & 0b11111];
        chars[0x03] = alphabet[(char)(number >>> 45) & 0b11111];
        chars[0x04] = alphabet[(char)(number >>> 40) & 0b11111];
        chars[0x05] = alphabet[(char)(number >>> 35) & 0b11111];
        chars[0x06] = alphabet[(char)(number >>> 30) & 0b11111];
        chars[0x07] = alphabet[(char)(number >>> 25) & 0b11111];
        chars[0x08] = alphabet[(char)(number >>> 20) & 0b11111];
        chars[0x09] = alphabet[(char)(number >>> 15) & 0b11111];
        chars[0x0a] = alphabet[(char)(number >>> 10) & 0b11111];
        chars[0x0b] = alphabet[(char)(number >>> 5) & 0b11111];
        chars[0x0c] = alphabet[(char)number & 0b11111];

        return new(chars);
    }

    private long GetTime() => _number >>> RANDOM_BITS;

    private long GetRandom() => _number & RANDOM_MASK;

    internal static class BaseN
    {
        static readonly BigInteger MAX = BigInteger.Subtract(BigInteger.Pow(new BigInteger(2), 64), BigInteger.One);
        static readonly string ALPHABET = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz"; // base-62

        internal static string Encode(SnowflakeId tsid, int baseN)
        {

            if (baseN < 2 || baseN > 62)
            {
                ThrowException(string.Format("Invalid base: %s", baseN));
            }

            var x = tsid._number;
            var b = Length(baseN);
            var buffer = new char[b];

            while (x.CompareTo(0) > 0)
            {
                var div = x / baseN;
                var rem = x % baseN;
                buffer[--b] = ALPHABET.ElementAt((int)rem);
                x = div;
            }

            while (b > 0)
            {
                buffer[--b] = '0';
            }

            return new string(buffer);
        }

        internal static SnowflakeId Decode(string value, int baseN)
        {

            if (value == null)
            {
                ThrowException($"Invalid base-{baseN} string: null");
            }

            if (baseN < 2 || baseN > 62)
            {
                ThrowException($"Invalid base: {baseN}");
            }

            var x = 0L;
            var last = 0L;
            var plus = 0L;

            var length = baseN;
            if (value.Length != length)
            {
                ThrowException($"Invalid base-{baseN} length: {value.Length}");
            }

            for (var i = 0; i < length; i++)
            {

                plus = ALPHABET.IndexOf(value.ElementAt(i));
                if (plus == -1)
                {
                    ThrowException(string.Format("Invalid base-%d character: %s", baseN, value.ElementAt(i)));
                }

                last = x;
                x = x * baseN + plus;
            }

            // finally, check if happened an overflow
            MemoryStream stream = new();
            stream.SetLength(8);
            stream.Write(BitConverter.GetBytes(last), 0, 8);
            var bytes = stream.ToArray();
            stream.Close();
            var lazt = BigInteger.Add(1, new BigInteger(bytes));
            BigInteger baze = new(baseN);
            BigInteger pluz = new(plus);
            if (BigInteger.Compare(BigInteger.Add(BigInteger.Multiply(lazt, baze), pluz), MAX) > 0)
            {
                throw new InvalidOperationException(string.Format("Invalid base-%d value (overflow): %s", baseN, lazt));
            }

            return new(x);
        }

        private static int Length(int value) => (int)Math.Ceiling(long.MaxValue / (Math.Log(value) / Math.Log(2)));

        private static void ThrowException(string value) => throw new InvalidOperationException(value);
    }

    internal static partial class Base32
    {
        private static readonly char[] DIGITS;
        private static readonly int MASK;
        private static readonly int SHIFT;
        private static Dictionary<char, int> CHAR_MAP = new();
        private const string SEPARATOR = "-";

        static Base32()
        {
            DIGITS = "ABCDEFGHIJKLMNOPQRSTUVWXYZ234567".ToCharArray();
            MASK = DIGITS.Length - 1;
            SHIFT = NumberOfTrailingZeros(DIGITS.Length);
            for (var i = 0; i < DIGITS.Length; i++) CHAR_MAP[DIGITS[i]] = i;
        }

        private static int NumberOfTrailingZeros(int i)
        {
            // HD, Figure 5-14
            int y;
            if (i == 0) return 32;
            var n = 31;
            y = i << 16; if (y != 0) { n -= 16; i = y; }
            y = i << 8; if (y != 0) { n -= 8; i = y; }
            y = i << 4; if (y != 0) { n -= 4; i = y; }
            y = i << 2; if (y != 0) { n -= 2; i = y; }
            return n - (int)((uint)(i << 1) >> 31);
        }

        public static byte[] Decode(string encoded)
        {
            // Remove whitespace and separators
            encoded = encoded.Trim().Replace(SEPARATOR, "");

            // Remove padding. Note: the padding is used as hint to determine how many
            // bits to decode from the last incomplete chunk (which is commented out
            // below, so this may have been wrong to start with).
            encoded = RemovePadding().Replace(encoded, "");

            // Canonicalize to all upper case
            encoded = encoded.ToUpper();
            if (encoded.Length == 0)
            {
                return Array.Empty<byte>();
            }

            var encodedLength = encoded.Length;
            var outLength = encodedLength * SHIFT / 8;
            var result = new byte[outLength];
            var buffer = 0;
            var next = 0;
            var bitsLeft = 0;
            foreach (var c in encoded.ToCharArray())
            {
                if (!CHAR_MAP.ContainsKey(c))
                {
                    throw new DecodingException("Illegal character: " + c);
                }
                buffer <<= SHIFT;
                buffer |= CHAR_MAP[c] & MASK;
                bitsLeft += SHIFT;
                if (bitsLeft >= 8)
                {
                    result[next++] = (byte)(buffer >> bitsLeft - 8);
                    bitsLeft -= 8;
                }
            }
            // We'll ignore leftover bits for now.
            //
            // if (next != outLength || bitsLeft >= SHIFT) {
            //  throw new DecodingException("Bits left: " + bitsLeft);
            // }
            return result;
        }

        public static string Encode(byte[] data, bool padOutput = false)
        {
            if (data.Length == 0)
            {
                return "";
            }

            // SHIFT is the number of bits per output character, so the length of the
            // output is the length of the input multiplied by 8/SHIFT, rounded up.
            if (data.Length >= 1 << 28)
            {
                // The computation below will fail, so don't do it.
                throw new ArgumentOutOfRangeException(nameof(data));
            }

            var outputLength = (data.Length * 8 + SHIFT - 1) / SHIFT;
            var result = new StringBuilder(outputLength);

            int buffer = data[0];
            var next = 1;
            var bitsLeft = 8;
            while (bitsLeft > 0 || next < data.Length)
            {
                if (bitsLeft < SHIFT)
                {
                    if (next < data.Length)
                    {
                        buffer <<= 8;
                        buffer |= data[next++] & 0xff;
                        bitsLeft += 8;
                    }
                    else
                    {
                        var pad = SHIFT - bitsLeft;
                        buffer <<= pad;
                        bitsLeft += pad;
                    }
                }

                var index = MASK & buffer >> bitsLeft - SHIFT;
                bitsLeft -= SHIFT;
                result.Append(DIGITS[index]);
            }

            if (padOutput)
            {
                var padding = 8 - result.Length % 8;
                if (padding > 0) result.Append(new string('=', padding == 8 ? 0 : padding));
            }

            return result.ToString();
        }

        private class DecodingException : Exception
        {
            public DecodingException(string message) : base(message)
            {
            }
        }

        [GeneratedRegex("[=]*$")]
        private static partial Regex RemovePadding();
    }
}
