using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Task.Connector.Extensions
{
    public static class ConnectionStringExtension
    {
        public static string GetDbConnectionString (this string connectionString)
        {
            Regex csPattern = new Regex(@"ConnectionString='.*?'", RegexOptions.IgnoreCase);
            Match match = csPattern.Match (connectionString);
            string dbConnectionString = match.Value.Replace("ConnectionString='", "").TrimEnd('\'');
            return dbConnectionString;
        }
        public static string GetProviderName(this string connectionString)
        {
            if (connectionString.Contains("Provider='SqlServer"))
            {
                return "MSSQL";
            }
            if (connectionString.Contains("Provider='PostgreSQL"))
            {
                return "POSTGRE";
            }
            else
            {
                return "UnknownProvider";
            }
        }
    }
}
