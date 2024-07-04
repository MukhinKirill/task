namespace Task.Connector.Extensions;

internal static class StringExtensions
{
    public static string ToFormat(this string value) => @"""" + value + @"""";
}