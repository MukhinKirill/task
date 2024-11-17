using CommunityToolkit.Diagnostics;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using System.Reflection;
using Task.Integration.Data.DbCommon.DbModels;
using Task.Integration.Data.Models.Models;


namespace Task.Connector.Extensions
{
    public static class UserExtension
    {
        public static void SetUserProperies(this User value, IEnumerable<UserProperty> properties)
        {
            if (properties.Count() > 0)
            {
                Dictionary<string, PropertyInfo> userPropertyDict = DataContextExtension.GetDbModelProperties(typeof(User));

                Type userPropertyValueType = nameof(UserProperty.Value).GetType();

                foreach (UserProperty userProperty in properties)
                {
                    if (!userPropertyDict.TryGetValue(userProperty.Name.ToLower(), out PropertyInfo userPropertyInfo))
                    {
                        ThrowHelper.ThrowInvalidDataException($"Поле {userProperty.Name} отсутсвует в таблице {nameof(User)}");
                    }

                    TypeConverter typeConverter = TypeDescriptor.GetConverter(userPropertyInfo.PropertyType);

                    if (typeConverter == null
                        || !typeConverter.CanConvertFrom(userPropertyValueType)
                       )
                    {
                        ThrowHelper.ThrowInvalidDataException($"Тип поля {userProperty.Name} не соответствует типу поля в таблице {nameof(User)}");
                    }

                    if (userPropertyInfo.PropertyType == typeof(string))
                    {
                        MaxLengthAttribute stringMaxLenghtAttr = userPropertyInfo.GetCustomAttribute<MaxLengthAttribute>()!;
                        if (stringMaxLenghtAttr != null
                            && !stringMaxLenghtAttr.IsValid(userProperty.Value)
                            )
                        {
                            ThrowHelper.ThrowInvalidDataException($"Размер значения не умещается в поле {userProperty.Name} ({stringMaxLenghtAttr.Length}) в таблице {nameof(User)} ");
                        }
                    }
                    userPropertyInfo.SetValue(value, typeConverter.ConvertFrom(userProperty.Value));
                }
            }
        }
    }
}
