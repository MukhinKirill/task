namespace Task.Connector.Models.Schemas
{
    // Предоставляет схему данных конкретной разновидности прав пользователя,
    // хранимых в БД
    public class PermissionSchema
    {
        public bool ContainsDescription { get; }
        public string PermissionIdName { get; }
        public string? DescriptionColumnName { get; }
        public string PermissionTypeTableName { get; }
        public string UserPermissionTableName { get; }

        public string GroupName { get; }
        public string Delimeter { get; }

        public PermissionSchema(string permissionTypeTableName, string userPermissionTableName,
            bool containsDescription, string permissionIdName, string groupName, string delimeter, string? descriptionColumnName = null)
        {
            if (containsDescription && descriptionColumnName == null)
            {
                throw new ArgumentNullException("containsDescription is set to true, but no descriptionColumnName is provided");
            }

            ContainsDescription = containsDescription;
            PermissionIdName = permissionIdName;
            DescriptionColumnName = descriptionColumnName;
            PermissionTypeTableName = permissionTypeTableName;
            UserPermissionTableName = userPermissionTableName;
            GroupName = groupName;
            Delimeter = delimeter;
        }
    }
}
