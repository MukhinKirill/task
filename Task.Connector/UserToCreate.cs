﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Task.Connector
{
    public class UserToCreate
    {
        public string Login { get; set; }
        public string Name { get; set; }
        public string LastName { get; set; }

        public string FirstName { get; set; }

        public string MiddleName { get; set; }

        public string TelephoneNumber { get; set; }

        public string IsLead { get; set; }
    }
}
