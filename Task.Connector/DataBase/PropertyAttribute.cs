using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Task.Integration.Data.Models.Models;

namespace Task.Connector.DataBase
{
    #region RecordClasses
    /// <summary>
    /// record-класс
    /// </summary>
    internal class DbItemPropertyInfo
    {
        public DbItemPropertyInfo(string name, string description) 
        {
            Name = name;
            Description = description;
        }
        /// <summary>
        /// Имя свойства
        /// </summary>
        public string Name { get; init; }
        /// <summary>
        /// Описание свойства
        /// </summary>
        public string Description { get; init; }
    }

    /// <summary>
    /// record-класс
    /// </summary>
    internal class DbItemPropertyWithValueInfo: DbItemPropertyInfo
    {
        public DbItemPropertyWithValueInfo(string name, string description, object value):base(name, description)
        {
            Value = value;
        }
        /// <summary>
        /// Значение свойства
        /// </summary>
        public object Value { get; init; }
    }
    #endregion //RecordClasses

    /// <summary>
    /// Полезные инструменты для работы со свойствами классов-таблиц, имещими аттрибут DbItemPropertyAttribute
    /// </summary>
    internal static class DbItemPropertyTools
    {
        /// <summary>
        /// Возвращает все найденные свойства (в т.ч. и вложенные) для заполнения для данного типа
        /// </summary>
        /// <param name="type">Тип</param>
        /// <returns>Свойства</returns>
        public static IEnumerable<DbItemPropertyInfo> GetAllProperties(Type type)
        {
            List<DbItemPropertyInfo> result = new();
            var properties = type.GetProperties()
                .Select(p => new { property = p, attr = p.GetCustomAttribute<DbItemPropertyAttribute>() })
                .Where(p => p?.attr != null);
            foreach ( var property in properties)
            {
                //Если свойство сложное - обрабатываем его отдельно
                if (property.attr.IsInto)
                    result.AddRange(GetAllProperties(property.property.PropertyType));
                else
                    result.Add(new(property.attr.Name, property.attr.Description));
            }
            return result;
        }

        /// <summary>
        /// Возвращает все найденные свойства для заполнения и их значения для данного объекта
        /// </summary>
        /// <param name="obj">Объект</param>
        /// <returns>Свойства</returns>
        public static IEnumerable<DbItemPropertyWithValueInfo> GetAllPropertiesOnlyObject(object obj)
        {
            List<DbItemPropertyWithValueInfo> result = new();
            var properties = obj.GetType().GetProperties()
                .Select(p => new { property = p, attr = p.GetCustomAttribute<DbItemPropertyAttribute>() })
                .Where(p => p?.attr != null);
            foreach (var property in properties)
            {
                //Если свойство сложное - игнорируем
                if (!property.attr.IsInto)
                    result.Add(new(property.attr.Name, property.attr.Description, property.property.GetValue(obj)));
            }
            return result;
        }
    }

    /// <summary>
    /// Аттрибут для обозначения свойства для заполнения. Оно может быть как нешним, так и внутреним
    /// При отсутствии параметров в конструкторе свойство обозначает, что внутри этого свойства есть другие свойства 
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
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
        /// <summary>
        /// Имя свойства. Дублирует имя столбца в таблице
        /// </summary>
        public string? Name { get; init; }

        /// <summary>
        /// Описание свойства
        /// </summary>
        public string? Description { get; init; }

        /// <summary>
        /// Флаг. Являеся ли свойство внутренним (внутри свойства есть ли класс со свойствами)?
        /// </summary>
        public bool IsInto { get; init; } = false;
    }
}
