using System.Collections;
using Snowflake.Net.Internals;

namespace Snowflake.Net;

[Serializable]
public sealed partial class SnowflakeId : IEquatable<SnowflakeId>, IComparable<SnowflakeId>, IComparable
{
    public SnowflakeId(long number) => _number = number;
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
    public long GetUnixMilliseconds() => GetTime + SNOWFLAKEID_EPOCH;
    public long GetUnixMilliseconds(long customEpoch) => GetTime + customEpoch;
    public string Encode(int value) => value < 2 || value > 62 ? throw new ArgumentException($"Invalid base: {value}") : BaseN.Encode(this, value);
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
    public int CompareTo(SnowflakeId other)
    {
        var a = _number + long.MinValue;
        var b = other._number + long.MinValue;

        if (a > b)
            return 1;
        else if (a < b)
            return -1;

        return 0;
    }
    public int CompareTo(object obj)
    {
        if (obj is null) return 1;
        if (ReferenceEquals(this, obj)) return 0;
        return obj is SnowflakeId other ? CompareTo(other) : throw new ArgumentException($"Object must be of type {nameof(SnowflakeId)}");
    }
    public bool Equals(SnowflakeId other)
    {
        if (other is SnowflakeId value)
            return _number == value._number;
        return false;
    }
    public override bool Equals(object obj) => ReferenceEquals(this, obj) || obj is SnowflakeId other && Equals(other);
    public override int GetHashCode() => (int)(_number ^ (_number >>> 32));

    public static bool operator <(SnowflakeId left, SnowflakeId right) => Comparer<SnowflakeId>.Default.Compare(left, right) < 0;

    public static bool operator >(SnowflakeId left, SnowflakeId right) => Comparer<SnowflakeId>.Default.Compare(left, right) > 0;

    public static bool operator <=(SnowflakeId left, SnowflakeId right) => Comparer<SnowflakeId>.Default.Compare(left, right) <= 0;

    public static bool operator >=(SnowflakeId left, SnowflakeId right) => Comparer<SnowflakeId>.Default.Compare(left, right) >= 0;

    public static bool operator ==(SnowflakeId left, SnowflakeId right) => Equals(left, right);

    public static bool operator !=(SnowflakeId left, SnowflakeId right) => !Equals(left, right);
}