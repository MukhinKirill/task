using System.Text.RegularExpressions;

namespace Task.Connector;

public class ConnectionStringParser
{
    public static string GetPostgreConnectionString(string fullString)
    {
        var regexPattern = @"ConnectionString='([^']+)'";
        var match = Regex.Match(fullString, regexPattern);
        if (match.Success)
        {
            return match.Groups[1].Value;
        }
        else
        {
            throw new ArgumentException("Invalid connection string format");
        }
    }
}