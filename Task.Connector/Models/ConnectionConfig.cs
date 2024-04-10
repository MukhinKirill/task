
namespace Task.Connector.Models
{
    internal class ConnectionConfig
    {
        private readonly string _connectionString;

        public ConnectionConfig(string connectionString)
        {
            _connectionString = connectionString;

            Provider = GetValueFromConStr("Provider");
            SchemaName = GetValueFromConStr("SchemaName");
        }

        public string Provider { get; } = null!;

        public string SchemaName { get; } = null!;

        private string GetValueFromConStr(string key)
        {
            string searchString = key + "='";
            int startIndex = _connectionString.IndexOf(searchString, StringComparison.OrdinalIgnoreCase);

            if (startIndex == -1)
            {
                return string.Empty;
            }

            startIndex += searchString.Length;
            int endIndex = _connectionString.IndexOf('\'', startIndex);

            if (endIndex == -1)
            {
                return string.Empty; 
            }

            return _connectionString.Substring(startIndex, endIndex - startIndex);
        }
    }
}
