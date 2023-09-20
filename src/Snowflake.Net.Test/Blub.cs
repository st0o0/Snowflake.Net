using Xunit;

namespace Snowflake.Net.Test;

public class Blub
{
    [Fact]
    public void GetId()
    {
        var idOne = new SnowflakeId(123);
        var IdTwo = new SnowflakeId(123);
        Assert.NotEqual(idOne, IdTwo);
    }

    [Fact]
    public void BlubTest()
    {
        Assert.True(true);
    }
}
