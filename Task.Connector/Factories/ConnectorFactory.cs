using Task.Connector.Connectors;
using Task.Connector.Constants;
using Task.Connector.Interfaces;

namespace Task.Connector.Factories
{
    internal static class ConnectorFactory
    {
        public static IConnectorDb GetConnector(string provider)
        {
            return provider switch
            {
                ProviderConstants.MSSQL => new MssqlConnector(),
                ProviderConstants.POSTGRE => new PostgreConnector(),
                _ => throw new ArgumentException("Provider don't registration - {0}", nameof(provider)),
            };
        }
    }
}
