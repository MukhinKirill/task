using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Task.Connector.Model
{
    internal class UserRequestRight
    {
        public int UserId { get; set; }
        public int RequestRightId { get; set; }
        public string Value { get; set; } = string.Empty;

        // relations
        public User User { get; set; }
        public RequestRight RequestRight { get; set; }
    }
}
