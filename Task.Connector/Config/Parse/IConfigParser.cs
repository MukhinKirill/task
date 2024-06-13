using System.Text;

namespace Task.Connector.Config.Parse;

public class ConnectionScheme
{
    public string Domain { get; set; } = "";
    public string Database { get; set; } = "";
    public string Username { get; set; } = "";
    public string Password { get; set; } = "";
    public string Host { get; set; } = "";
    public int Port { get; set; } = -1;
    public IReadOnlyDictionary<string, string>? Parameters { get; set; } = null;
    public override string ToString()
    {
        var sb = new StringBuilder();
        sb.Append(Domain).Append("://");
        if (Username.Length > 0)
        {
            sb.Append(Username);
            if (Password.Length > 0)
            {
                sb.Append(':').Append(Password);
            }
        }


        if (Host.Length > 0)
        {
            if (Username.Length > 0)
            {
                sb.Append('@');
            }

            sb.Append(Host);
        }

        if (Port > 0)
        {
            sb.Append(':').Append(Port);
        }

        if (!String.IsNullOrWhiteSpace(Database))
        {
            sb.Append('/').Append(Database);
        }

        if (Parameters?.Count > 0)
        {
            int i = 0;
            const string delim = "?&";
            foreach (var (name, value) in Parameters)
            {
                sb.Append(delim[i]);
                i = Math.Min(i + 1, delim.Length - 1);
                sb.Append(name).Append('=').Append(value);
            }
        }

        return sb.ToString();
    }
}

public interface IConfigParser
{
    public ConnectionScheme Parse(string s);
}