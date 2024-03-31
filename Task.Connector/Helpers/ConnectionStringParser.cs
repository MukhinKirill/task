using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Task.Connector.Constants;
using Task.Connector.DataBaseContext;
using Task.Connector.Enums;

namespace Task.Connector.Helpers
{
    public static class ConnectionStringParser
    {
        public static DataBaseProvider GetProvider(string connectionString)
        {
            return connectionString.Contains("POSTGRESQL",StringComparison.InvariantCultureIgnoreCase)
                ? DataBaseProvider.POSTGRE: connectionString.Contains("SqlServer", StringComparison.InvariantCultureIgnoreCase)
                ? DataBaseProvider.MSSQL:DataBaseProvider.Default;
        } 
        public static string GetConnectionString(DataBaseProvider provider)
        {
            switch (provider)
            {
                case Enums.DataBaseProvider.POSTGRE:
                    return DataBaseConnectionStrings.PostgreConnectionString;
                case Enums.DataBaseProvider.MSSQL:
                    return DataBaseConnectionStrings.DefaultConnectionString;
                default:
                    return DataBaseConnectionStrings.DefaultConnectionString;
            }
        }
    }
}
