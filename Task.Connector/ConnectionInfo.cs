using System.Text.RegularExpressions;

namespace Task.Connector
{
    /// <summary>
    /// Класс содержащий информацию из строки подключения к БД
    /// </summary>
    public class ConnectionInfo
    {
        private const string PostgreProviderName = "POSTGRE";
        private const string MsSqlProviderName = "MSSQL";

        private readonly string _provider;
        private readonly string _schema;
        private readonly string _connectionString;

        public string Provider => _provider;
        public string Schema => _schema;
        public string ConnectionString => _connectionString;

        public ConnectionInfo(string connectionString)
        {
            var properties = Parse(connectionString);

            _provider = GetProviderName(properties["Provider"]);
            _schema = properties["SchemaName"];
            _connectionString = properties["ConnectionString"];
        }

        private Dictionary<string, string> Parse(string connectionString)
        {
            var regex = new Regex(@"(?<key>\w+)='(?<value>[^']+)';");
            var properties = regex.Matches(connectionString);

            return properties.ToDictionary(x => x.Groups["key"].Value, x => x.Groups["value"].Value);
        }

        private string GetProviderName(string provider)
        {
            if (provider.Contains("PostgreSQL"))
            {
                return PostgreProviderName;
            }

            if (provider.Contains("SqlServer"))
            {
                return MsSqlProviderName;
            }

            return string.Empty;
        }
    }
}
