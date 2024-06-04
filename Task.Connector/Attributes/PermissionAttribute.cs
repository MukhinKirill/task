using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Task.Connector.Attributes
{
    public class PermissionAttribute : Attribute
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }

        public PermissionAttribute(int id, string name, string description) 
        {
            Id = id;
            Name = name;
            Description = description;
        }
    }
}
