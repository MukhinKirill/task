using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TR.Connectors.Api.Entities.DTO.Connection;

namespace TR.Connector
{
    public static class ConnectionStringParser
    {
        public static ConnectionOptions Parse(string connectionString)
        {
            if (string.IsNullOrWhiteSpace(connectionString))
                throw new ArgumentNullException("connectionString не может быть пустой", nameof(connectionString));

            string? url = null;
            string? login = null;
            string? password = null;

            foreach (var item in connectionString.Split(';', StringSplitOptions.RemoveEmptyEntries))
            {
                var idx = item.IndexOf('=');
                if (idx <= 0) continue;

                var key = item.Substring(0, idx).Trim();
                var value = item.Substring(idx + 1).Trim();

                if (string.Equals(key, "url", StringComparison.OrdinalIgnoreCase)) url = value;
                else if (string.Equals(key, "login", StringComparison.OrdinalIgnoreCase)) login = value;
                else if (string.Equals(key, "password", StringComparison.OrdinalIgnoreCase)) password = value;
            }

            if (string.IsNullOrWhiteSpace(url))
                throw new ArgumentException("connectionString must contain 'url' entry", nameof(connectionString));

            return new ConnectionOptions(url, login ?? string.Empty, password ?? string.Empty);
        }
    }
}
