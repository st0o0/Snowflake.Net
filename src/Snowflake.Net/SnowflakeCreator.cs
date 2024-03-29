﻿namespace Snowflake.Net;

public class SnowflakeCreator
{
	/// <summary>
	/// Returns a new SnowflakeId.
	/// <para>
	/// The node ID is is set by defining the system property "snowflakeidcreator.node" or
	/// the environment variable "SNOWFLAKEIDCREATOR_NODE". One of them <b>should</b> be
	/// used to embed a machine ID in the generated SnowflakeId in order to avoid SnowflakeId
	/// collisions. If that property or variable is not defined, the node ID is
	/// chosen randomly.
	/// </para>
	/// <para>
	/// The amount of nodes can be set by defining the system property
	/// "snowflakeidcreator.node.count" or the environment variable
	/// "SNOWFLAKEIDCREATOR_NODE_COUNT". That property or variable is used to adjust the
	/// minimum amount of bits to accommodate the node ID. If that property or
	/// variable is not defined, the default amount of nodes is 1024, which takes 10
	/// bits.
	/// </para>
	/// The amount of bits needed to accommodate the node ID is calculated by this
	/// pseudo-code formula: {@code node_bits = ceil(log(node_count)/log(2))}.
	/// <para>
	/// Random component settings:
	/// <ul>
	/// <li>Node bits: node_bits</li>
	/// <li>Counter bits: 22-node_bits</li>
	/// <li>Maximum node: 2^node_bits</li>
	/// <li>Maximum counter: 2^(22-node_bits)</li>
	/// </ul>
	/// </para>
	/// <para>
	/// The time component can be 1 ms or more ahead of the system time when
	/// necessary to maintain monotonicity and generation speed.
	/// </para>
	/// <returns>a <see cref="SnowflakeId"/></returns>
	/// </summary>
	public static SnowflakeId GetSnowflakeId() => FactoryHolder.INSTANCE.Create();

	/// <summary>
	/// Returns a new SnowflakeId.
	/// <para>
	/// It supports up to 256 nodes.
	/// </para>
	/// It can generate up to 16,384 SnowflakeId per millisecond per node.
	/// <para>
	/// The node ID is is set by defining the system property "snowflakeidcreator.node" or
	/// the environment variable "SNOWFLAKEIDCREATOR_NODE". One of them <b>should</b> be
	/// used to embed a machine ID in the generated SnowflakeId in order to avoid SnowflakeId
	/// collisions. If that property or variable is not defined, the node ID is
	/// chosen randomly.
	/// </para>
	/// <para>
	/// Random component settings:
	/// <ul>
	/// <li>Node bits: 8 </li>
	/// <li>Counter bits: 14 </li>
	/// <li>Maximum node: 256 (2^8) </li>
	/// <li>Maximum counter: 16,384 (2^14) </li>
	/// </ul>
	/// </para>
	/// <para>
	/// The time component can be 1 ms or more ahead of the system time when
	/// necessary to maintain monotonicity and generation speed.
	/// </para>
	/// <returns>a <see cref="SnowflakeId"/></returns>
	/// </summary>
	public static SnowflakeId GetSnowflakeId256() => Factory256Holder.INSTANCE.Create();

	/// <summary>
	/// <para>
	/// Returns a new SnowflakeId.
	/// </para>
	/// <para>
	/// It supports up to 1,024 nodes.
	/// </para>
	/// <para>
	/// It can generate up to 4,096 SnowflakeIds per millisecond per node.
	/// </para>
	/// <para>
	/// The node ID is is set by defining the system property "snowflakeidcreator.node" or
	/// the environment variable "SNOWFLAKEIDCREATOR_NODE". One of them <b>should</b> be
	/// used to embed a machine ID in the generated SnowflakeId in order to avoid SnowflakeId
	/// collisions. If that property or variable is not defined, the node ID is
	/// chosen randomly.
	/// </para>
	/// <para>
	/// Random component settings:
	/// <ul>
	/// <li>Node bits: 10 </li>
	/// <li>Counter bits: 12 </li>
	/// <li>Maximum node: 1,024 (2^10) </li>
	/// <li>Maximum counter: 4,096 (2^12) </li>
	/// </ul>
	/// </para>
	/// <para>
	/// The time component can be 1 ms or more ahead of the system time when
	/// necessary to maintain monotonicity and generation speed.
	/// </para>
	/// <returns>a <see cref="Tsid"/></returns>
	/// </summary>
	public static SnowflakeId GetSnowflakeId1024() => Factory1024Holder.INSTANCE.Create();

	/// <summary>
	/// <para>
	/// Returns a new SnowflakeId.
	/// </para>
	/// <para>
	/// It supports up to 4,096 nodes.
	/// </para>
	/// <para>
	/// It can generate up to 1,024 SnowflakeIds per millisecond per node.
	/// </para>
	/// <para>
	/// The node ID is is set by defining the system property "snowflakeidcreator.node" or
	/// the environment variable "SNOWFLAKEIDCREATOR_NODE". One of them <b>should</b> be
	/// used to embed a machine ID in the generated SnowflakeId in order to avoid SnowflakeId
	/// collisions. If that property or variable is not defined, the node ID is
	/// chosen randomly.
	/// </para>
	/// <para>
	/// Random component settings:
	/// <ul>
	/// <li>Node bits: 12 </li>
	/// <li>Counter bits: 10 </li>
	/// <li>Maximum node: 4,096 (2^12) </li>
	/// <li>Maximum counter: 1,024 (2^10) </li>
	/// </ul>
	/// </para>
	/// <para>
	/// The time component can be 1 ms or more ahead of the system time when
	/// necessary to maintain monotonicity and generation speed.
	/// </para>
	/// <returns>a <see cref="SnowflakeId"/></returns>
	/// </summary>
	public static SnowflakeId GetSnowflakeId4096() => Factory4096Holder.INSTANCE.Create();

	private static class FactoryHolder
	{
		internal static readonly SnowflakeFactory INSTANCE = new();
	}

	private static class Factory256Holder
	{
		internal static readonly SnowflakeFactory INSTANCE = SnowflakeFactory.NewInstance256();
	}

	private static class Factory1024Holder
	{
		internal static readonly SnowflakeFactory INSTANCE = SnowflakeFactory.NewInstance1024();
	}

	private static class Factory4096Holder
	{
		internal static readonly SnowflakeFactory INSTANCE = SnowflakeFactory.NewInstance4096();
	}
}
