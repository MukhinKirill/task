namespace Task.Connector.Models.Schemas
{
    // Предоставляет схему данных пользователя, хранимых в БД
    public class UserSchema
    {
        public Dictionary<string, string> PropertyTypes { get; }
        public string UserTableName { get; }
        public string PasswordTableName { get; }

        public UserSchema(string userTableName, string passwordTableName, Dictionary<string, string> propertyTypes)
        {
            if (propertyTypes.ContainsKey("string"))
            {
                throw new ArgumentException("Cannot declare login property in a user schema");
            }
            if (propertyTypes.ContainsKey("password"))
            {
                throw new ArgumentException("Cannot declare password property in a user schema");
            }

            PropertyTypes = propertyTypes;
            UserTableName = userTableName;
            PasswordTableName = passwordTableName;
        }
    }
}
