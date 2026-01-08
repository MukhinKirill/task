namespace TR.Connector.Helpers;

public class ConnectionStringResult
{
    public string Url { get; init; } = string.Empty;
    public string Login { get; init; } = string.Empty;
    public string Password { get; init; } = string.Empty;
}

public static class ConnectionStringParser
{
    public static ConnectionStringResult Parse(string connectionString)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(connectionString, nameof(connectionString));

        var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        foreach (var part in connectionString.Split(';', StringSplitOptions.RemoveEmptyEntries))
        {
            var trimmedPart = part.Trim();
            var equalsIndex = trimmedPart.IndexOf('=');

            if (equalsIndex <= 0)
                continue;

            var key = trimmedPart[..equalsIndex].Trim();
            var value = trimmedPart[(equalsIndex + 1)..].Trim();

            result[key] = value;
        }

        if (!result.TryGetValue("url", out var url) || string.IsNullOrWhiteSpace(url))
            throw new ArgumentException("Connection string must contain 'url' parameter");

        if (!result.TryGetValue("login", out var login) || string.IsNullOrWhiteSpace(login))
            throw new ArgumentException("Connection string must contain 'login' parameter");

        if (!result.TryGetValue("password", out var password))
            throw new ArgumentException("Connection string must contain 'password' parameter");

        return new ConnectionStringResult
        {
            Url = url.TrimEnd('/'),
            Login = login,
            Password = password
        };
    }
}
