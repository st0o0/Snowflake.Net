using System;

namespace Snowflake.Net;

public class SnowflakeCreator
{
	public static SnowflakeId GetSnowflakeId() {
		return FactoryHolder.INSTANCE.Create();
	}

	/**
	 * Returns a new TSID.
	 * <p>
	 * It supports up to 256 nodes.
	 * <p>
	 * It can generate up to 16,384 TSIDs per millisecond per node.
	 * <p>
	 * The node ID is is set by defining the system property "tsidcreator.node" or
	 * the environment variable "TSIDCREATOR_NODE". One of them <b>should</b> be
	 * used to embed a machine ID in the generated TSID in order to avoid TSID
	 * collisions. If that property or variable is not defined, the node ID is
	 * chosen randomly.
	 * 
	 * <p>
	 * Random component settings:
	 * <ul>
	 * <li>Node bits: 8
	 * <li>Counter bits: 14
	 * <li>Maximum node: 256 (2^8)
	 * <li>Maximum counter: 16,384 (2^14)
	 * </ul>
	 * <p>
	 * The time component can be 1 ms or more ahead of the system time when
	 * necessary to maintain monotonicity and generation speed.
	 * 
	 * @return a TSID
	 */
	public static SnowflakeId GetSnowflakeId256() {
		return Factory256Holder.INSTANCE.Create();
	}

	/**
	 * Returns a new TSID.
	 * <p>
	 * It supports up to 1,024 nodes.
	 * <p>
	 * It can generate up to 4,096 TSIDs per millisecond per node.
	 * <p>
	 * The node ID is is set by defining the system property "tsidcreator.node" or
	 * the environment variable "TSIDCREATOR_NODE". One of them <b>should</b> be
	 * used to embed a machine ID in the generated TSID in order to avoid TSID
	 * collisions. If that property or variable is not defined, the node ID is
	 * chosen randomly.
	 * <p>
	 * Random component settings:
	 * <ul>
	 * <li>Node bits: 10
	 * <li>Counter bits: 12
	 * <li>Maximum node: 1,024 (2^10)
	 * <li>Maximum counter: 4,096 (2^12)
	 * </ul>
	 * <p>
	 * The time component can be 1 ms or more ahead of the system time when
	 * necessary to maintain monotonicity and generation speed.
	 * 
	 * @return a TSID
	 */
	public static SnowflakeId GetSnowflakeId1024() {
		return Factory1024Holder.INSTANCE.Create();
	}

	/**
	 * Returns a new TSID.
	 * <p>
	 * It supports up to 4,096 nodes.
	 * <p>
	 * It can generate up to 1,024 TSIDs per millisecond per node.
	 * <p>
	 * The node ID is is set by defining the system property "tsidcreator.node" or
	 * the environment variable "TSIDCREATOR_NODE". One of them <b>should</b> be
	 * used to embed a machine ID in the generated TSID in order to avoid TSID
	 * collisions. If that property or variable is not defined, the node ID is
	 * chosen randomly.
	 * <p>
	 * Random component settings:
	 * <ul>
	 * <li>Node bits: 12
	 * <li>Counter bits: 10
	 * <li>Maximum node: 4,096 (2^12)
	 * <li>Maximum counter: 1,024 (2^10)
	 * </ul>
	 * <p>
	 * The time component can be 1 ms or more ahead of the system time when
	 * necessary to maintain monotonicity and generation speed.
	 * 
	 * @return a TSID number
	 */
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
