using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Task.Connector
{
    public static class ConnectorHelper
    {
        public static Exception LastException { get; set; }
        public static string DefaultConnectionString(string _ConnectionString)
        {
            try
            {
                Match match = Regex.Match(_ConnectionString, @"ConnectionString='(.*?)'");

                return match.Groups[1].Value;           
            }
            catch(Exception ex)
            {
                LastException = ex;
                return null;
            }
        }
        public static string GetProvider(string _ConnectionString)
        {

            if (_ConnectionString.ToLower().Contains("sqlserver", StringComparison.InvariantCultureIgnoreCase))
                return "MSSQL";

            else if (_ConnectionString.ToLower().Contains("postgresql", StringComparison.InvariantCultureIgnoreCase))
                return "POSTGRE";

            else
                return null;
        }
    }
}
