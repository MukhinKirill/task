using System.Reflection;
using Task.Connector.Attributes;
using Task.Integration.Data.Models.Models;

namespace Task.Connector.Converters
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
