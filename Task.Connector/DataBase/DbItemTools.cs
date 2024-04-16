using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Task.Connector.DataBase
{
    /// <summary>
    /// Класс полезных функций для работы с полями в классах-таблицах
    /// </summary>
    internal static class DbItemTools
    {
        /// <summary>
        /// Устанавливает значение в объект target по имени свойства или по имени связанного с ним столбца таблицы БД. Регистронезависимый
        /// </summary>
        /// <param name="target">Объект, в который вставляется</param>
        /// <param name="name">Имя столбца или свойства</param>
        /// <param name="value">Значение</param>
        /// <returns>Удачна ли вставка</returns>
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

        /// <summary>
        /// Вставляет в свойство объекта значение. Было создано для разгрузки кода
        /// </summary>
        /// <param name="property">Свойство</param>
        /// <param name="target">Объект</param>
        /// <param name="value">Значение</param>
        /// <returns>Удачна ли вставка</returns>
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
