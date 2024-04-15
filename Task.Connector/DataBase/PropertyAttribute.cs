using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Task.Integration.Data.Models.Models;

namespace Task.Connector.DataBase
{

    internal class DbItemPropertyInfo
    {
        public DbItemPropertyInfo(string name, string description) 
        {
            Name = name;
            Description = description;
        }
        public string Name { get; init; }
        public string Description { get; init; }
    }

    internal class DbItemPropertyWithValueInfo: DbItemPropertyInfo
    {
        public DbItemPropertyWithValueInfo(string name, string description, object value):base(name, description)
        {
            Value = value;
        }
        public object Value { get; init; }
    }

    internal static class DbItemPropertyTools
    {
        public static IEnumerable<DbItemPropertyInfo> GetAllProperties(Type type)
        {
            List<DbItemPropertyInfo> result = new();
            var properties = type.GetProperties()
                .Select(p => new { property = p, attr = p.GetCustomAttribute<DbItemPropertyAttribute>() })
                .Where(p => p?.attr != null);
            foreach ( var property in properties)
            {
                if (property.attr.IsInto)
                    result.AddRange(GetAllProperties(property.property.PropertyType));
                else
                    result.Add(new(property.attr.Name, property.attr.Description));
            }
            return result;
        }

        public static IEnumerable<DbItemPropertyWithValueInfo> GetAllPropertiesOnlyObject(object obj)
        {
            List<DbItemPropertyWithValueInfo> result = new();
            var properties = obj.GetType().GetProperties()
                .Select(p => new { property = p, attr = p.GetCustomAttribute<DbItemPropertyAttribute>() })
                .Where(p => p?.attr != null);
            foreach (var property in properties)
            {
                if (!property.attr.IsInto)
                    result.Add(new(property.attr.Name, property.attr.Description, property.property.GetValue(obj)));
            }
            return result;
        }
    }
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
