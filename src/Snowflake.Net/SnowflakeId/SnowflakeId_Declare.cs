namespace Snowflake.Net;

public sealed partial class SnowflakeId
{
    private readonly long _number = 0L;
    private static int _atomic = new Random().Next();
    internal static readonly int SNOWFLAKEID_BYTES = 8;
    internal static readonly int SNOWFLAKEID_CHARS = 13;
    internal static readonly long SNOWFLAKEID_EPOCH = (long)(DateTime.Parse("2023-01-01T00:00:00.000Z").ToUniversalTime() - new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc)).TotalMilliseconds;
    internal static readonly int RANDOM_BITS = 22;
    internal static readonly int RANDOM_MASK = 0x003fffff;
    internal long GetTime => _number >>> RANDOM_BITS;

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
}
