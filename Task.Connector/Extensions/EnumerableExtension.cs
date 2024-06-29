using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Task.Integration.Data.Models.Models;

namespace Task.Connector.Extensions
{
    public static class EnumerableExtension
    {
        public static Dictionary<string, string> ConvertToDict(this IEnumerable<UserProperty> properties)
        {
            var result = new Dictionary<string, string>();

            foreach (var property in properties)
            {
                result.Add(property.Name, property.Value);
            }

            return result;
        }

        public static Dictionary<string, string> ConvertToDict(this IEnumerable<Property> properties)
        {
            var result = new Dictionary<string, string>();

            foreach (var property in properties)
            {
                result.Add(property.Name, property.Description);
            }

            return result;
        }

        public static string? GetPropertyColumnName(this IEnumerable<IProperty> properties, string name)
        {
            return properties.Where(property => property.Name == name).FirstOrDefault()?.GetColumnName();
        }

        public static string GetPropertyColumnNameOrDefault(this IEnumerable<IProperty> properties, string name, string @default)
        {
            return properties.GetPropertyColumnName(name) ?? @default;
        }
    }
}
