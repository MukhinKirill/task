using System.Text.RegularExpressions;
using Npgsql;
using Task.Connector.Resources;

namespace Task.Connector.DomainModels;

public static class ConnectionStringParser
{
    private static readonly Regex  Host = new (@"Host=[0-9a-zA-z.]+;", RegexOptions.Compiled | RegexOptions.IgnoreCase);
    private static readonly Regex  Port = new (@"Port=\d+;", RegexOptions.Compiled | RegexOptions.IgnoreCase);
    private static readonly Regex  DbName = new (@"Database=[0-9a-zA-z.]+;", RegexOptions.Compiled | RegexOptions.IgnoreCase);
    private static readonly Regex  Username = new (@"Username=[0-9a-zA-z]+;", RegexOptions.Compiled | RegexOptions.IgnoreCase);
    private static readonly Regex  Password = new (@"Password=[0-9a-zA-z]*;", RegexOptions.Compiled | RegexOptions.IgnoreCase);
    private static readonly Regex  SchemaName = new (@"SchemaName=[0-9a-zA-z.']+;", RegexOptions.Compiled | RegexOptions.IgnoreCase);
    
    public static string ParseConnectionString(string connectionstring)
    {
        NpgsqlConnectionStringBuilder builder = new();
        
        Match match = PatternCheck(connectionstring, Host, nameof(Host));
        builder.Host = match.Captures[0].Value.Split('=')[1].TrimEnd(';');
        
        match = PatternCheck(connectionstring, Port, nameof(Port));
        builder.Port = int.Parse(match.Captures[0].Value.Split('=')[1].TrimEnd(';'));
        
        match = PatternCheck(connectionstring, DbName, nameof(DbName));
        builder.Database = match.Captures[0].Value.Split('=')[1].TrimEnd(';');
        
        match = PatternCheck(connectionstring, Username, nameof(Username));
        builder.Username = match.Captures[0].Value.Split('=')[1].TrimEnd(';');
        
        match = PatternCheck(connectionstring, Password, nameof(Password));
        var passwordKeyValue = match.Captures[0].Value.Split('=');
        builder.Password = passwordKeyValue.Length != 2 ? string.Empty : passwordKeyValue[1].TrimEnd(';');
        
        match = SchemaName.Match(connectionstring);
        builder.SearchPath = !match.Success ? "public" : match.Captures[0].Value.Split('=')[1].TrimEnd(';').Trim('\'');
        
        return builder.ConnectionString;
    }

    private static Match PatternCheck(string connectionstring, Regex pattern, string patternName)
    {
        var match = pattern.Match(connectionstring);
        if (!match.Success) throw new FormatException(string.Format(
            ExceptionMessages.IncorrectConnectionStringParam,
            patternName
        ));
        return match;
    }
}