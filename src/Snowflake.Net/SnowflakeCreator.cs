using System;

namespace Snowflake.Net;

public class SnowflakeCreator
{
	public static SnowflakeId GetSnowflakeId() {
		return FactoryHolder.INSTANCE.Create();
	}

	public static SnowflakeId GetSnowflakeId256() {
		return Factory256Holder.INSTANCE.Create();
	}

	public static SnowflakeId GetSnowflakeId1024() {
		return Factory1024Holder.INSTANCE.Create();
	}

	public static SnowflakeId GetSnowflakeId4096() {
		return Factory4096Holder.INSTANCE.Create();
	}

	private static class FactoryHolder {
		internal static readonly SnowflakeFactory INSTANCE = new();
	}

	private static class Factory256Holder {
		internal static readonly SnowflakeFactory INSTANCE = SnowflakeFactory.NewInstance256();
	}

	private static class Factory1024Holder {
		internal static readonly SnowflakeFactory INSTANCE = SnowflakeFactory.NewInstance1024();
	}

	private static class Factory4096Holder {
		internal static readonly SnowflakeFactory INSTANCE = SnowflakeFactory.NewInstance4096();
	}
}
