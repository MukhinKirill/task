namespace Task.Connector.Helpers
{
    internal class PermissionParser
    {
        private static PermissionType GetTypeFromString(string permissionType)
        {
            return permissionType switch
            {
                "Request" => PermissionType.RequestRight,
                "Role" => PermissionType.Role,
                null => throw new ArgumentNullException(nameof(permissionType)),
                _ => throw new NotImplementedException(),
            };
        }

        public static (List<int> roles, List<int> rights) Parse(IEnumerable<string> rawPermissions, string delimeter = ":")
        {
            var roles = new List<int>();
            var rights = new List<int>();
            foreach (var rawPermission in rawPermissions)
            {
                var split = rawPermission.Split(delimeter);
                var type = GetTypeFromString(split[0]);
                var id = int.Parse(split[1]);
                switch (type)
                {
                    case PermissionType.Role:
                        roles.Add(id);
                        break;
                    case PermissionType.RequestRight:
                        rights.Add(id);
                        break;
                }
            }

            return (roles, rights);
        }
    }
}
