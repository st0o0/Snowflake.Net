using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using DotNext.Threading;

namespace Snowflake.Net
{
    [Serializable]
    public sealed class SnowflakeId : EqualityComparer<SnowflakeId>
    {
        private static readonly long serialVersionUID = -5446820982139116297L;
        private readonly long _number;

        private static int _atomic = new Random().Next();

        internal static readonly int TSID_BYTES = 8;
        internal static readonly int TSID_CHARS = 13;
        public static readonly long TSID_EPOCH = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).Millisecond;

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

            long number = 0;

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

        public static SnowflakeId From(string value)
        {

            char[] chars = ToCharArray(value);

            long number = 0;

            number |= ALPHABET_VALUES[chars[0x00]] << 60;
            number |= ALPHABET_VALUES[chars[0x01]] << 55;
            number |= ALPHABET_VALUES[chars[0x02]] << 50;
            number |= ALPHABET_VALUES[chars[0x03]] << 45;
            number |= ALPHABET_VALUES[chars[0x04]] << 40;
            number |= ALPHABET_VALUES[chars[0x05]] << 35;
            number |= ALPHABET_VALUES[chars[0x06]] << 30;
            number |= ALPHABET_VALUES[chars[0x07]] << 25;
            number |= ALPHABET_VALUES[chars[0x08]] << 20;
            number |= ALPHABET_VALUES[chars[0x09]] << 15;
            number |= ALPHABET_VALUES[chars[0x0a]] << 10;
            number |= ALPHABET_VALUES[chars[0x0b]] << 5;
            number |= ALPHABET_VALUES[chars[0x0c]];

            return new(number);
        }

        public static SnowflakeId Decode(string value, int number) => BaseN.Decode(value, number);

        public static SnowflakeId Fast()
        {
            long time = (Internals.System.CurrentTimeMillis() - TSID_EPOCH) << RANDOM_BITS;
            long tail = AtomicInt32.IncrementAndGet(ref _atomic) & RANDOM_MASK;
            return new SnowflakeId(time | tail);
        }

        public long ToLong() => this._number;

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
                int i = format.IndexOf("%");
                if (i < 0 || i == format.Length - 1)
                {
                    throw new InvalidOperationException(string.Format("Invalid format string: \"%s\"", format));
                }
                string head = format[..i];
                string tail = format[(i + 2)..];
                char placeholder = format.ElementAt(i + 1);
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
                    _ => throw new InvalidOperationException(string.Format("Invalid placeholder: \"%%%s\"", placeholder)),
                };
            }
            throw new InvalidOperationException(string.Format("Invalid format string: \"%s\"", format));
        }

        public static SnowflakeId Unformat(string formatted, string format)
        {
            if (formatted != null && format != null)
            {
                int i = format.IndexOf("%");
                if (i < 0 || i == format.Length - 1)
                {
                    throw new InvalidOperationException(string.Format("Invalid format string: \"%s\"", format));
                }
                string head = format[..i];
                string tail = format[(i + 2)..];
                char placeholder = format.ElementAt(i + 1);
                int length = formatted.Length - head.Length - tail.Length;
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
                        _ => throw new InvalidOperationException(message: string.Format("Invalid placeholder: \"%%%s\"", placeholder)),
                    };
                }
            }
            throw new InvalidOperationException(message: string.Format("Invalid formatted string: \"%s\"", formatted));
        }

        public override bool Equals(SnowflakeId value, SnowflakeId other)
        {
            if (other == null || value == null)
                return false;
            if (other.GetType() != typeof(SnowflakeId) || value.GetType() != typeof(SnowflakeId))
                return false;
            return value._number == other._number;
        }

        public override int GetHashCode(SnowflakeId obj)
        {
            return (int)(_number ^ (_number >>> 32));
        }

        static char[] ToCharArray(string value)
        {
            char[] chars = value?.ToCharArray();
            if (!IsValidCharArray(chars))
            {
                throw new InvalidOperationException(string.Format("Invalid TSID string: \"%s\"", value));
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

            for (int i = 0; i < chars.Length; i++)
            {
                if (!ALPHABET_VALUES.ContainsKey(chars[i]))
                {
                    return false; // invalid character!
                }
                continue;
            }
            return true; // It seems to be OK.
        }

        static string ToString(char[] alphabet, long number)
        {
            char[] chars = new char[TSID_CHARS];

            chars[0x00] = alphabet[(int)((number >>> 60) & 0b11111)];
            chars[0x01] = alphabet[(int)((number >>> 55) & 0b11111)];
            chars[0x02] = alphabet[(int)((number >>> 50) & 0b11111)];
            chars[0x03] = alphabet[(int)((number >>> 45) & 0b11111)];
            chars[0x04] = alphabet[(int)((number >>> 40) & 0b11111)];
            chars[0x05] = alphabet[(int)((number >>> 35) & 0b11111)];
            chars[0x06] = alphabet[(int)((number >>> 30) & 0b11111)];
            chars[0x07] = alphabet[(int)((number >>> 25) & 0b11111)];
            chars[0x08] = alphabet[(int)((number >>> 20) & 0b11111)];
            chars[0x09] = alphabet[(int)((number >>> 15) & 0b11111)];
            chars[0x0a] = alphabet[(int)((number >>> 10) & 0b11111)];
            chars[0x0b] = alphabet[(int)((number >>> 5) & 0b11111)];
            chars[0x0c] = alphabet[(int)(number & 0b11111)];

            return new string(chars);
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

                long x = tsid._number;
                int b = Length(baseN);
                char[] buffer = new char[b];

                while (x.CompareTo(0) > 0)
                {
                    long div = x / baseN;
                    long rem = x % baseN;
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
                    ThrowException(string.Format("Invalid base-%d string: null", baseN));
                }
                if (baseN < 2 || baseN > 62)
                {
                    ThrowException(string.Format("Invalid base: %s", baseN));
                }

                long x = 0L;
                long last = 0;
                long plus = 0;

                int length = baseN;
                if (value.Length != length)
                {
                    ThrowException(string.Format("Invalid base-%d length: %s", baseN, value.Length));
                }

                for (int i = 0; i < length; i++)
                {

                    plus = ALPHABET.IndexOf(value.ElementAt(i));
                    if (plus == -1)
                    {
                        ThrowException(string.Format("Invalid base-%d character: %s", baseN, value.ElementAt(i)));
                    }

                    last = x;
                    x = (x * baseN) + plus;
                }

                // finally, check if happened an overflow
                MemoryStream stream = new();
                stream.SetLength(8);
                stream.Write(BitConverter.GetBytes(last), 0, 8);
                byte[] bytes = stream.ToArray();
                stream.Close();
                BigInteger lazt = BigInteger.Add(1, new BigInteger(bytes));
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
    }
}
