using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Task.Connector.Model
{
    internal class User
    {
        public int Id { get; set; }
        public string Login { get; set; } = string.Empty;

        // relations
        public virtual ICollection<UserRequestRight> UserRequestRights { get; set; }
        public virtual ICollection<UserItRole> UserItRoles { get; set; }
    }
}
