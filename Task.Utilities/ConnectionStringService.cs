using System.Text.RegularExpressions;

namespace Task.Utilities
{
    public class ConnectionStringService
    {
        public static string GetConnectionString(string connectionString)
        {
            string pattern = @"ConnectionString='(.*?)'";
            string connection = string.Empty;

            Match match = Regex.Match(connectionString, pattern);
            if (match.Success)
            {
                connection = match.Groups[1].Value;
            }
            return connection;
        }

        public static Provider GetProvider(string connectionString)
        {
            string pattern = @"Provider='([^']*)'";
            string provider = string.Empty;

            Match match = Regex.Match(connectionString, pattern);
            if (match.Success)
            {
                provider = match.Groups[1].Value.ToUpper();

                if(provider == "SQLSERVER.2019") return Provider.MSSQL;
            }
            return Provider.UNKNOWN;
        }
    }
}