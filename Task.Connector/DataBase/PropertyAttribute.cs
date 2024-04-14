using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Task.Connector.DataBase
{
    internal class DbItemPropertyAttribute : Attribute
    {
        public DbItemPropertyAttribute()
        {
            IsInto = true;
        }
        public DbItemPropertyAttribute(string name, string description)
        {
            Description = description;
            Name = name;
        }


        public string? Name { get; init; }

        public string? Description { get; init; }
        public bool IsInto { get; init; } = false;
    }
}
