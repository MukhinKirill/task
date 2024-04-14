using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Task.Connector.DataBase
{
    internal static class DbItemTools
    {
        internal static bool TrySetDbItemProperty(Object target, string name, object value)
        {
            name = name.ToLower();
            var type = target.GetType();
            var properties = type.GetProperties();

            foreach (var property in properties)
            {
                var columnName = property.GetCustomAttribute<ColumnAttribute>()?.Name;
                if (columnName != null)
                {
                    if (columnName.ToLower() == name)
                    {
                        return TrySetProperty(property, target, value);
                    }
                }
                else
                {
                    if (property.Name.ToLower() == name)
                    {
                        return TrySetProperty(property, target, value);
                    }
                }
            }
            return false;
        }
        private static bool TrySetProperty(PropertyInfo property, object target, object value)
        {
            try
            {
                property.SetValue(target, value);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
