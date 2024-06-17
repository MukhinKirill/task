using System.Reflection;
using Task.Integration.Data.Models.Models;
using UserEntity = Task.Integration.Data.DbCommon.DbModels.User;

namespace Task.Connector.Services.User
{
    public static class UserHelper
    {
        private static readonly Dictionary<Type, string> _defaultDataForType;

        static UserHelper()
        {
            _defaultDataForType = new Dictionary<Type, string>()
            {
                [typeof(string)] = string.Empty,
                [typeof(bool)] = "false"
            };
        }

        public static UserEntity InitializeUser(UserEntity userEntity, IEnumerable<UserProperty> properties)
        {
            userEntity = SetUserProperties(userEntity, properties);
            userEntity = InitializeUserWithDefaultData(userEntity);
            return userEntity;
        }

        private static UserEntity SetUserProperties(UserEntity userEntity, IEnumerable<UserProperty> userProperties)
        {
            foreach (var userProperty in userProperties)
            {
                var property = userEntity
                    .GetType()
                    .GetProperty(userProperty.Name, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);

                if (property is null)
                {
                    throw new Exception($"User doesn't have property with name = {userProperty.Name}");
                }

                property.SetValue(userEntity, Convert.ChangeType(userProperty.Value, property.PropertyType));
            }

            return userEntity;
        }

        private static UserEntity InitializeUserWithDefaultData(UserEntity userEntity)
        {
            var userProperties = userEntity
                .GetType()
                .GetProperties()
                .Where(p =>
                    p.GetCustomAttribute(typeof(System.ComponentModel.DataAnnotations.KeyAttribute)) is null &&
                    IsDefaultValue(p.GetValue(userEntity)));

            foreach (var userProperty in userProperties)
            {
                _defaultDataForType.TryGetValue(userProperty.PropertyType, out var defaultTypeValue);
                if (defaultTypeValue is null)
                {
                    throw new Exception($"Default data for type {userProperty.PropertyType} wasn't set");
                }

                var convertedTypeValue = Convert.ChangeType(defaultTypeValue, userProperty.PropertyType);
                userProperty.SetValue(userEntity, convertedTypeValue);
            }

            return userEntity;
        }

        private static bool IsDefaultValue(object value)
        {
            if (value is null)
            {
                return true;
            }

            if (value.GetType().IsValueType)
            {
                return value.Equals(Activator.CreateInstance(value.GetType()));
            }

            return false;
        }
    }
}