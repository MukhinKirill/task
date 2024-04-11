﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Task.Connector.Exceptions
{
    internal class UserNotFoundException : Exception
    {
        public UserNotFoundException(string userLogin) : base($"User not found by login - {userLogin}") { }
    }
}
