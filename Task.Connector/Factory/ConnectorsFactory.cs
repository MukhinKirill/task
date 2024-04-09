using Microsoft.EntityFrameworkCore;
using Task.Connector.Connectors;
using Task.Connector.DbModels;
using Task.Connector.Models;
using Task.Integration.Data.Models;

namespace Task.Connector.Factory
{
    internal class ConnectorsFactory
    {
        public static IConnector GetConnector(string provider)
        {
            //TODO: Get constant values out of a switch statement
            switch (provider)
            {
                case "SqlServer.2019":
                    return new MssqlConnector();
                case "PostgreSQL":
                    return new PsqlConnector();
                default:
                    throw new ArgumentException($"Unknown provider - {nameof(provider)}");
            }
        }
    }
}
