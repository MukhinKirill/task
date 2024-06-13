using System.Text.RegularExpressions;

namespace Task.Connector.Config.Parse;

public sealed class UrlParserFormatException : Exception{}


/// <summary>
/// <para>
/// This parser collects connection information from URL-like connection string.
/// It must have the following specification: <br/>
/// <c>domain://username:password@host[:port][/database][?param1=value1&amp;...&amp;param2=value2]</c> <br/>
///
/// <list type="bullet">
/// <item>
///     <term>Domain:</term>
///     <description>Database connection provider name. Examples: <c>postgres</c>, <c>mssql</c>, <c>mssql</c></description>
/// </item>
/// <item>
///     <term>Username, Password:</term>
///     <description>Username and password to use for connection to database</description>
/// </item>
/// <item>
///     <term>Host:</term>
///     <description>Database hostname</description>
/// </item>
/// <item>
///     <term>Port</term>
///     <description>Optional port number</description>
/// </item>
/// <item>
///     <term>Database</term>
///     <description>Optional database name</description>
/// </item>
/// <item>
///     <term>Parameter list:</term>
///     <description>Optional <c>&amp;</c>-separated query parameters list</description>
/// </item>
/// </list>
/// </para>
/// </summary>
public sealed class ConnectionUrlParser : IConfigParser
{
    public ConnectionUrlParser()
    {
    }

    /// <summary>
    /// Default parser instance
    /// </summary>
    public static ConnectionUrlParser Default { get; } = new ConnectionUrlParser();
    
    /// <summary>
    /// Parse URL-like connection string into <c>ConnectionScheme</c> object.
    /// <param name="s">URL-like connection string</param>
    /// <exception cref="UrlParserFormatException">Invalid, empty or unknown URL was given to parse</exception>
    /// </summary>
    public ConnectionScheme Parse(string s)
    {
        if (string.IsNullOrWhiteSpace(s))
        {
            throw new UrlParserFormatException();
        }

        Uri uri;
        try
        {
            uri = new Uri(s, dontEscape:true);
        }
        catch (Exception)
        {
            throw new UrlParserFormatException();
        }

        if (!s.Substring(uri.Scheme.Length).StartsWith("://"))
        {
            throw new UrlParserFormatException();
        }
        
        string query = uri.Query;
        string[] userInfo = uri.UserInfo.Split(':');
        string userName = userInfo.Length > 0 ? userInfo[0] : "";
        string userPass = userInfo.Length > 1 ? userInfo[1] : "";
        string database = uri.LocalPath.Substring(1); // to skip leading '/'

        Dictionary<string, string>? queryValues = null;
        if (query.Length > 0)
        {
            int l = 0;
            int r = 0;
            const string delims = "?&";
            queryValues = new Dictionary<string, string>();
            while (r <= query.Length)
            {
                if (r == query.Length || delims.Contains(query[r]))
                {
                    if (r - l > 0)
                    {
                        var pair = query.Substring(l, r - l).Split('=');
                        queryValues.Add(pair[0], pair[1]);
                    }

                    if (r == query.Length)
                    {
                        break;
                    }
                    
                    l = r + 1;
                }

                r++;
            }
        }

        string host = uri.Host;
        if (string.IsNullOrEmpty(host))
        {
            host = "127.0.0.1"; // default to localhost
        }
        
        return new ConnectionScheme
        {
            Domain = uri.Scheme,
            Host = uri.Host,
            Database = database,
            Username = userName,
            Password = userPass,
            Port = uri.Port,
            Parameters = queryValues
        };
    }
}