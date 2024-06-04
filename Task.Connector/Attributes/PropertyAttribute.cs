using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Task.Connector.Attributes
{
    public class PropertyAttribute : Attribute
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string DefaultValue { get; set; }

        public PropertyAttribute(string name, string description)
        {
            Name = name;
            Description = description;
        }
    }
}
