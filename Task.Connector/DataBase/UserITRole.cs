using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Task.Connector.DataBase
{
    public class UserITRole
    {
        public string UserId { get; set; }
        public User User { get; set; }

        public int RoleId { get; set; }
        public ItRole Role { get; set; }
    }
}
