using Xunit;

namespace Snowflake.Net.Test;

public class Blub
{
    [Fact]
    public void GetId()
    {
        var idOne = SnowflakeFactory.NewInstance4096().Create();
        var IdTwo = SnowflakeFactory.NewInstance4096().Create();
        Assert.NotEqual(idOne.ToLong(), IdTwo.ToLong());
    }

    [Fact]
    public void BlubTest()
    {
        Assert.True(true);
    }
}
