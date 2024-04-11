using Task.Connector.Connectors;
using Task.Integration.Data.Models;

namespace Task.Connector.Factory
{
    internal class ConnectorsFactory
    {
        public static IConnector GetConnector(string provider)
        {
            switch (provider)
            {
                case Constants.Constants.MSSQL:
                    return new MssqlConnector();
                case Constants.Constants.POSTGRE:
                    return new PsqlConnector();
                default:
                    throw new ArgumentException($"Unknown provider - {nameof(provider)}");
            }
        }
    }
}
