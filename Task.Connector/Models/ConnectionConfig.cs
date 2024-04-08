
namespace Task.Connector.Models
{
    internal class ConnectionConfig
    {
        private readonly string _connectionString;

        public ConnectionConfig(string connectionString)
        {
            _connectionString = connectionString;

            //TODO: Get provider and schema from the connection string 
        }

        public string Provider { get; } = null!;

        public string SchemaName { get; } = null!;
    }
}
