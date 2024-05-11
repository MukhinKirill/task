using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;


namespace Task.Connector.Utils
{
    public class ConnectionParser
    {
        public string GetConnectionProvaider(string connectionString)
        {
            
            string[] parts = connectionString.Split(';');
            foreach (string part in parts)
            {
                string trimmedPart = part.Trim();
                if (trimmedPart.StartsWith("Provider=", StringComparison.OrdinalIgnoreCase))
                {
                    string providerValue = trimmedPart.Substring("Provider=".Length);
                    if (providerValue.Contains("PostgreSQL", StringComparison.OrdinalIgnoreCase))
                    {
                        return "POSTGRE";
                    }
                    else if (providerValue.Contains("SqlServer", StringComparison.OrdinalIgnoreCase))
                    {
                        return "MSSQL";
                    }
                }
            }
            return "Unknown";
        }

        public string DBConnectionString(string connectionString)
        {
            string pattern = @"ConnectionString='(.*?)';Provider='";
            Match match = Regex.Match(connectionString, pattern);
            if (match.Success)
            {
                return match.Groups[1].Value;
            }
            return string.Empty;
        }
    }
}
