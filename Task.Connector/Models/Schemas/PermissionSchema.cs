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
        public string SchemaName { get; }

        public PermissionSchema(bool containsDescription, string permissionIdName, string? descriptionColumnName,
            string permissionTypeTableName, string userPermissionTableName, string schemaName)
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
            SchemaName = schemaName;
        }
    }
}
