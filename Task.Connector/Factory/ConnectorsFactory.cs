using Task.Integration.Data.Models;

namespace Task.Connector.Factory
{
    internal class ConnectorsFactory
    {
        public static IConnector GetConnector(string provider)
        {
            //TODO: Get constants out in a separate file AND repair default ctor
            switch (provider)
            {
                case "MSSQL":
                    return MssqlConnector();
                case "POSTGRE":
                    return PostgreConnector();
                default:
                    throw new ArgumentException($"Unknown provider - {nameof(provider)}");
            }
        }
    }
}
