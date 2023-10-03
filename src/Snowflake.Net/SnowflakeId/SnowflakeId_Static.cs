using Snowflake.Net.Internals;

namespace Snowflake.Net;

public sealed partial class SnowflakeId
{
    public static SnowflakeId From(long number) => new(number);
    public static SnowflakeId From(byte[] bytes)
    {
        if (bytes == null || bytes.Length != SNOWFLAKEID_BYTES)
        {
            throw new InvalidOperationException("Invalid SNOWFLAKEID bytes"); // null or wrong length!
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
    public static SnowflakeId From(string str)
    {
        var chars = ToCharArray(str);

        long number = 0;

        number |= AlphabetValues[chars[0x00]] << 60;
        number |= AlphabetValues[chars[0x01]] << 55;
        number |= AlphabetValues[chars[0x02]] << 50;
        number |= AlphabetValues[chars[0x03]] << 45;
        number |= AlphabetValues[chars[0x04]] << 40;
        number |= AlphabetValues[chars[0x05]] << 35;
        number |= AlphabetValues[chars[0x06]] << 30;
        number |= AlphabetValues[chars[0x07]] << 25;
        number |= AlphabetValues[chars[0x08]] << 20;
        number |= AlphabetValues[chars[0x09]] << 15;
        number |= AlphabetValues[chars[0x0a]] << 10;
        number |= AlphabetValues[chars[0x0b]] << 5;
        number |= AlphabetValues[chars[0x0c]];

        return From(number);
    }
    public static SnowflakeId Decode(string value, int number) => number < 2 || number > 62 ? throw new ArgumentException($"Invalid base: {value}") : BaseN.Decode(value, number);
    public static SnowflakeId Fast()
    {
        var time = Internals.System.CurrentTimeMillis() - SNOWFLAKEID_EPOCH << RANDOM_BITS;
        var tail = (long)LazyHolder.IncrementAndGet() & RANDOM_MASK;
        return From(time | tail);
    }
    public static SnowflakeId Unformat(string formatted, string format)
    {
        if (formatted != null && format != null)
        {
            var i = format.IndexOf("%");
            if (i < 0 || i == format.Length - 1)
            {
                throw new ArgumentException($"Invalid format string: \"{format}\"");
            }
            var head = format.Substring(0, i);
            var tail = format.Substring(i + 2);
            var placeholder = format[i + 1];
            var length = formatted.Length - head.Length - tail.Length;
            if (formatted.StartsWith(head) && formatted.EndsWith(tail))
            {
                var subStr = formatted.Substring(i, length);
                return placeholder switch
                {
                    // canonical string (case insensitive)
                    'S' => From(subStr),
                    // canonical string (case insensitive)
                    's' => From(subStr),
                    // hexadecimal (case insensitive)
                    'X' => BaseN.Decode(subStr.ToUpper(), 16),
                    // hexadecimal (case insensitive)
                    'x' => BaseN.Decode(subStr.ToUpper(), 16),
                    // base-10
                    'd' => BaseN.Decode(subStr, 10),
                    // base-62
                    'z' => BaseN.Decode(subStr, 62),
                    _ => throw new ArgumentException($"Invalid placeholder: \"%%{placeholder}\""),
                };
            }
        }

        throw new ArgumentException($"Invalid formatted string: \"{formatted}\"");
    }
    public static bool IsValid(string str) => str != null && IsValidCharArray(str.ToCharArray());
    private static char[] ToCharArray(string value)
    {
        var chars = value.ToCharArray();
        if (!IsValidCharArray(chars))
        {
            throw new InvalidOperationException($"Invalid SNOWFLAKEID string: {value}");
        }

        return chars;
    }
    private static bool IsValidCharArray(char[] chars)
    {
        if (chars == null || chars.Length != SNOWFLAKEID_CHARS)
        {
            return false;
        }

        if ((AlphabetValues[chars[0]] & 0b10000) != 0)
        {
            return false;
        }

        for (var i = 0; i < chars.Length; i++)
        {
            if (AlphabetValues[chars[i]] == -1)
            {
                return false;
            }
        }

        return true;
    }
    private static string ToString(char[] alphabet, long number)
    {
        var chars = new char[SNOWFLAKEID_CHARS];

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
}
