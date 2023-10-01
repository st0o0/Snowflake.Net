using Snowflake.Net.Internals;

namespace Snowflake.Net;

[Serializable]
public sealed partial class SnowflakeId //: IEquatable<Tsid>, IComparable<Tsid>, IComparable
{
    private readonly long _number = 0L;
    private static int _atomic = new Random().Next();
    internal static readonly int SNOWFLAKEID_BYTES = 8;
    internal static readonly int SNOWFLAKEID_CHARS = 13;
    internal static readonly long SNOWFLAKEID_EPOCH = (long)(DateTime.Parse("2020-01-01T00:00:00.000Z").ToUniversalTime() - new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc)).TotalMilliseconds;
    internal static readonly int RANDOM_BITS = 22;
    internal static readonly int RANDOM_MASK = 0x003fffff;

    #region Alphabet

    private static readonly char[] AlphabetUppercase =
     {
        '0',
        '1',
        '2',
        '3',
        '4',
        '5',
        '6',
        '7',
        '8',
        '9',
        'A',
        'B',
        'C',
        'D',
        'E',
        'F',
        'G',
        'H',
        'J',
        'K',
        'M',
        'N',
        'P',
        'Q',
        'R',
        'S',
        'T',
        'V',
        'W',
        'X',
        'Y',
        'Z'
     };

    private static readonly char[] AlphabetLowercase =
    {
        '0',
        '1',
        '2',
        '3',
        '4',
        '5',
        '6',
        '7',
        '8',
        '9',
        'a',
        'b',
        'c',
        'd',
        'e',
        'f',
        'g',
        'h',
        'j',
        'k',
        'm',
        'n',
        'p',
        'q',
        'r',
        's',
        't',
        'v',
        'w',
        'x',
        'y',
        'z'
    };

    private static readonly long[] AlphabetValues = new long[128];

    static SnowflakeId()
    {
        for (var i = 0; i < AlphabetValues.Length; i++)
        {
            AlphabetValues[i] = -1;
        }

        // Numbers
        AlphabetValues['0'] = 0x00;
        AlphabetValues['1'] = 0x01;
        AlphabetValues['2'] = 0x02;
        AlphabetValues['3'] = 0x03;
        AlphabetValues['4'] = 0x04;
        AlphabetValues['5'] = 0x05;
        AlphabetValues['6'] = 0x06;
        AlphabetValues['7'] = 0x07;
        AlphabetValues['8'] = 0x08;
        AlphabetValues['9'] = 0x09;
        // Lower case
        AlphabetValues['a'] = 0x0a;
        AlphabetValues['b'] = 0x0b;
        AlphabetValues['c'] = 0x0c;
        AlphabetValues['d'] = 0x0d;
        AlphabetValues['e'] = 0x0e;
        AlphabetValues['f'] = 0x0f;
        AlphabetValues['g'] = 0x10;
        AlphabetValues['h'] = 0x11;
        AlphabetValues['j'] = 0x12;
        AlphabetValues['k'] = 0x13;
        AlphabetValues['m'] = 0x14;
        AlphabetValues['n'] = 0x15;
        AlphabetValues['p'] = 0x16;
        AlphabetValues['q'] = 0x17;
        AlphabetValues['r'] = 0x18;
        AlphabetValues['s'] = 0x19;
        AlphabetValues['t'] = 0x1a;
        AlphabetValues['v'] = 0x1b;
        AlphabetValues['w'] = 0x1c;
        AlphabetValues['x'] = 0x1d;
        AlphabetValues['y'] = 0x1e;
        AlphabetValues['z'] = 0x1f;
        // Lower case OIL
        AlphabetValues['o'] = 0x00;
        AlphabetValues['i'] = 0x01;
        AlphabetValues['l'] = 0x01;
        // Upper case
        AlphabetValues['A'] = 0x0a;
        AlphabetValues['B'] = 0x0b;
        AlphabetValues['C'] = 0x0c;
        AlphabetValues['D'] = 0x0d;
        AlphabetValues['E'] = 0x0e;
        AlphabetValues['F'] = 0x0f;
        AlphabetValues['G'] = 0x10;
        AlphabetValues['H'] = 0x11;
        AlphabetValues['J'] = 0x12;
        AlphabetValues['K'] = 0x13;
        AlphabetValues['M'] = 0x14;
        AlphabetValues['N'] = 0x15;
        AlphabetValues['P'] = 0x16;
        AlphabetValues['Q'] = 0x17;
        AlphabetValues['R'] = 0x18;
        AlphabetValues['S'] = 0x19;
        AlphabetValues['T'] = 0x1a;
        AlphabetValues['V'] = 0x1b;
        AlphabetValues['W'] = 0x1c;
        AlphabetValues['X'] = 0x1d;
        AlphabetValues['Y'] = 0x1e;
        AlphabetValues['Z'] = 0x1f;
        // Upper case OIL
        AlphabetValues['O'] = 0x00;
        AlphabetValues['I'] = 0x01;
        AlphabetValues['L'] = 0x01;
    }

    #endregion

    public SnowflakeId(long number) => _number = number;




    public static bool IsValid(string str) => str != null && IsValidCharArray(str.ToCharArray());

    public long ToLong() => _number;

    public byte[] ToBytes()
    {
        var result = new byte[SNOWFLAKEID_BYTES];

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

    public string ToLower() => ToString(AlphabetLowercase, _number);

    public DateTimeOffset GetDateTimeOffset() => DateTimeOffset.FromUnixTimeMilliseconds(GetUnixMilliseconds());

    public DateTimeOffset GetDateTimeOffset(DateTimeOffset customEpoch) => DateTimeOffset.FromUnixTimeMilliseconds(GetUnixMilliseconds(customEpoch.ToUnixTimeMilliseconds()));

    public long GetUnixMilliseconds() => GetTime() + SNOWFLAKEID_EPOCH;

    public long GetUnixMilliseconds(long customEpoch) => GetTime() + customEpoch;

    public string Encode(int value) => BaseN.Encode(this, value);

    public string Format(string format)
    {
        if (format != null)
        {
            var i = format.IndexOf("%");
            if (i < 0 || i == format.Length - 1)
            {
                throw new ArgumentException($"Invalid format string: \"{format}\"");
            }
            var head = format.Substring(0, i);
            var tail = format.Substring(i + 2);
            var placeholder = format[i + 1];
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
                _ => throw new ArgumentException($"Invalid placeholder: \"%%{placeholder}\""),
            };
        }

        throw new ArgumentException($"Invalid format string: \"{format}\"");
    }

    public override string ToString() => ToString(AlphabetUppercase, _number);

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

    private long GetTime() => _number >>> RANDOM_BITS;

    private long GetRandom() => _number & RANDOM_MASK;

    private static class LazyHolder
    {
        private static int _counter = StaticRandom.Instance.Next();

        public static int IncrementAndGet()
        {
            return Interlocked.Increment(ref _counter);
        }
    }

    private class DecodingException : Exception
    {
        public DecodingException(string message) : base(message) { }
    }
}