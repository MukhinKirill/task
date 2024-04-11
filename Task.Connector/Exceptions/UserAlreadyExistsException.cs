using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Task.Connector.Exceptions
{
    internal class UserAlreadyExistsException : Exception
    {
        public UserAlreadyExistsException(string userLogin) : base($"User with login:{userLogin} already exists") { }
    }
}
