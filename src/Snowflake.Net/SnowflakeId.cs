using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;

namespace Snowflake.Net
{
    [Serializable]
    public sealed class SnowflakeId : EqualityComparer<SnowflakeId>
    {
        private static readonly long serialVersionUID = -5446820982139116297L;
        private readonly long _number;

        internal static readonly int TSID_BYTES = 8;
        internal static readonly int TSID_CHARS = 13;
        public static readonly long TSID_EPOCH = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).Millisecond;

        private static readonly char[] ALPHABET_UPPERCASE = //
			{ '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', //
					'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'J', 'K', //
					'M', 'N', 'P', 'Q', 'R', 'S', 'T', 'V', 'W', 'X', 'Y', 'Z' };

        private static readonly char[] ALPHABET_LOWERCASE = //
                { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', //
					'a', 'b', 'c', 'd', 'e', 'f', 'g', 'h', 'j', 'k', //
					'm', 'n', 'p', 'q', 'r', 's', 't', 'v', 'w', 'x', 'y', 'z' };


        private static readonly Dictionary<char, byte> ALPHABET_VALUES = new Dictionary<char, byte>()
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

        public static SnowflakeId From(long number) => new SnowflakeId(number);

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

            return new SnowflakeId(number);
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

            return new SnowflakeId(number);
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

        public override bool Equals(SnowflakeId x, SnowflakeId y)
        {
            // todo: KEKW
            return false;
        }

        public override int GetHashCode(SnowflakeId obj)
        {
            // todo: KEKW
            return 0;
        }

        public static class BaseN
        {
            static readonly BigInteger MAX = BigInteger.Subtract(BigInteger.Pow(new BigInteger(2), 64), BigInteger.One);
            static readonly string ALPHABET = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz"; // base-62

            static string Encode(SnowflakeId tsid, int baseN)
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

            static SnowflakeId Decode(string value, int baseN)
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
                MemoryStream stream = new MemoryStream();
                stream.SetLength(8);
                stream.Write(BitConverter.GetBytes(last), 0, 8);
                byte[] bytes = stream.ToArray();
                stream.Close();
                BigInteger lazt = BigInteger.Add(1, new BigInteger(bytes));
                BigInteger baze = new BigInteger(baseN);
                BigInteger pluz = new BigInteger(plus);
                if (BigInteger.Compare(BigInteger.Add(BigInteger.Multiply(lazt, baze), pluz), MAX) > 0)
                {
                    throw new InvalidOperationException(string.Format("Invalid base-%d value (overflow): %s", baseN, lazt));
                }

                return new SnowflakeId(x);
            }

            private static int Length(int value)
            {
                return (int)Math.Ceiling(long.MaxValue / (Math.Log(value) / Math.Log(2)));
            }

            private static void ThrowException(string value)
            {
                throw new InvalidOperationException(value);
            }
        }

    }


}
