using System.Text.RegularExpressions;

namespace Task.Connector.Models
{
    public class ConnectionConfiguration
    {
        private readonly string _connectionString;

        public ConnectionConfiguration(string connectionString)
        {
            _connectionString = connectionString;

            ConnectionString = GetValue("ConnectionString");
            Provider = GetValue("Provider");
            SchemaName = GetValue("SchemaName");
        }

        public string ConnectionString { get; } = null!;

        public string Provider { get; } = null!;

        public string SchemaName { get; } = null!;

        private string GetValue(string key)
        {
            var regex = new Regex(@$"{key}='\s*(?<{key}>[^']+)'", RegexOptions.IgnoreCase);
            var groups = regex.Match(_connectionString);

            return groups.Groups[key].Value;
        }
    }
}
