using System.ComponentModel.DataAnnotations;

namespace Task.Connector.Helpers.Property
{
    internal static class PropertyHelper
    {
        public static IEnumerable<Task.Integration.Data.Models.Models.Property> GetProperties(Type type, params string[] exclude)
        {
            var properties = type.GetProperties()
                .Where(p => p.CanWrite && !exclude.Contains(p.Name)).ToList()
                .Select(p =>
                {
                    var maxLengthAttribute = p.GetCustomAttributes(typeof(MaxLengthAttribute), false)
                        .FirstOrDefault() as MaxLengthAttribute;
                    var description = string.Join(", ", new[]
                    {
                        p.PropertyType.ToString(),
                        maxLengthAttribute != null ? $"{maxLengthAttribute.GetType().Name}({maxLengthAttribute.Length})" : ""
                    });
                    return new Task.Integration.Data.Models.Models.Property
                    (
                        name: p.Name,
                        description: description
                    );
                })
                .ToList();
            return properties;
        }
    }
}
