using Xunit;

namespace Snowflake.Net.Test;

public class SnowflakeIdTests
{

    [Fact]
    public void GetTwoIds()
    {
        var idOne = SnowflakeFactory.NewInstance256().Create();
        var idTwo = SnowflakeFactory.NewInstance256().Create();
        Assert.NotEqual(idOne.ToLong(), idTwo.ToLong());
    }

    [Fact]
    public void GetOneIdAndParseItToLongAndBackToSnowflakeId()
    {
        var idOne = SnowflakeFactory.NewInstance256().Create();
        var valueLong = idOne.ToLong();
        Assert.IsType<long>(valueLong);
        var result = SnowflakeId.From(valueLong);
        Assert.IsType<SnowflakeId>(result);
        Assert.Equal(idOne.ToLong(), result.ToLong());
    }

    [Fact]
    public void GetOneIdAndParseItToByteAndBackToSnowflakeId()
    {
        var idOne = SnowflakeFactory.NewInstance256().Create();
        var value = idOne.ToBytes();
        Assert.IsType<byte[]>(value);
        Assert.Equal(8, value.Length);
        var result = SnowflakeId.From(value);
        Assert.Equal(idOne.ToLong(), result.ToLong());
    }

    [Fact]
    public void GetOneIdAndParseItToStringAndBackToSnowflakeId()
    {
        var idOne = SnowflakeFactory.NewInstance256().Create();
        var value = idOne.ToString();
        Assert.IsType<string>(value);
        var result = SnowflakeId.From(value);
        Assert.Equal(idOne.ToLong(), result.ToLong());
    }
}
