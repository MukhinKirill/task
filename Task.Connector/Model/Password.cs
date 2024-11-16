using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Task.Connector.Model
{
    internal class Password
    {
        public int Id { get; set; }
        public string UserLogin { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
    }
}
