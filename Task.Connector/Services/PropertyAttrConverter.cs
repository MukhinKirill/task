using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Task.Connector.Attributes;
using Task.Connector.Models;
using Task.Integration.Data.Models.Models;

namespace Task.Connector.Services
{
    public class PropertyAttrConverter
    {
        public List<Property> GetAttributesFromType(Type type)
        {

            var propertiess = new List<Property>();
            PropertyInfo[] properties = type.GetProperties();
            foreach (var property in properties)
            {
                var attributes = property.GetCustomAttributes(true);

                foreach (PropertyAttribute attribute in attributes)
                {
                    propertiess.Add(new Property(attribute.Name, attribute.Description));
                }
            }
            return propertiess;
        } 
    }
}
