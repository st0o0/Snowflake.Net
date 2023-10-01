namespace Snowflake.Net;

internal static class EnvironmentSettings
{
    public const string Node = "snowflakeid.node";
    public const string NodeCount = "snowflakeid.node.count";

    public static int? GetNode()
    {
        return GetPropertyAsInteger(Node);
    }

    public static int? GetNodeCount()
    {
        return GetPropertyAsInteger(NodeCount);
    }

    private static int? GetPropertyAsInteger(string property)
    {
        try
        {
            return int.Parse(GetProperty(property));
        }
        catch (FormatException)
        {
            return null;
        }
        catch (ArgumentNullException)
        {
            return null;
        }
    }

    private static string GetProperty(string name)
    {
        var property = Environment.GetEnvironmentVariable(name.ToUpper().Replace(".", "_"));
        if (!string.IsNullOrEmpty(property))
        {
            return property;
        }

        property = Environment.GetEnvironmentVariable(name);
        if (!string.IsNullOrEmpty(property))
        {
            return property;
        }

        return null;
    }
}
