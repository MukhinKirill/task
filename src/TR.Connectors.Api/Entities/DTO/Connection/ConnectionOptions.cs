using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TR.Connectors.Api.Entities.DTO.Connection
{
    public sealed class ConnectionOptions
    {
        public string Url { get; }
        public string Login { get; }
        public string Password { get; }

        public ConnectionOptions(string url, string login, string password)
        {
            Url = url ?? throw new ArgumentNullException(nameof(url));
            Login = login ?? string.Empty;
            Password = password ?? string.Empty;
        }
    }
}
