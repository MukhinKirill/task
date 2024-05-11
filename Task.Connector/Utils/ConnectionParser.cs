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
            /*var pairs = connectionString.Split(';', StringSplitOptions.RemoveEmptyEntries)
                             .Select(pair => pair.Split('='))
                             .ToDictionary(pair => pair[0].Trim(), pair => pair[1].Trim());
            var pairs = connectionString.Split(';', StringSplitOptions.RemoveEmptyEntries)
                 .Select(pair => pair.Split('='))
                 .Where(pair => pair.Length == 2) // Убедимся, что пара содержит и ключ, и значение
                 .ToDictionary(pair => pair[0].Trim(), pair => pair[1].Trim());

            string provaider = pairs.ContainsKey("Provaider") ? pairs["Provaider"] : "Unknown";

            return provaider;*/

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
                    // Добавьте другие провайдеры, если они используются
                }
            }
            return "Unknown";
        }


        public string GetUsername(string connectionString)
        {
            string username;
            string provaiderName = GetConnectionProvaider(connectionString);

            if (provaiderName == "MSSSQL")
                username = new SqlConnectionStringBuilder(connectionString).UserID;
            else if (provaiderName == "POSTGRE")
                username = new Npgsql.NpgsqlConnectionStringBuilder(connectionString).Username;
            else
                throw new Exception("Неизвестный провайдер базы данных!");

            return username;
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
