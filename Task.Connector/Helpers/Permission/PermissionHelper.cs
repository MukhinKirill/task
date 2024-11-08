using System.Text.RegularExpressions;
using Task.Integration.Data.DbCommon.DbModels;

namespace Task.Connector.Helpers.Permission
{
    internal static class PermissionHelper
    {
        public static (IEnumerable<int> rightIds, IEnumerable<int> roleIds) ParsePermissionStrings(IEnumerable<string> permissions)
        {
            var roles = new List<int>();
            var rights = new List<int>();

            string pattern = @"^(?<type>[a-zA-Z]+)[^\w]*(?<id>\d+)$";
            foreach (var str in permissions)
            {
                var match = Regex.Match(str, pattern);
                if (match.Success)
                {
                    string type = match.Groups["type"].Value.ToLower();
                    string id = match.Groups["id"].Value;

                    if (type.Contains("it") || type.Contains("role"))
                    {
                        roles.Add(int.Parse(id));
                    }
                    else if (type.Contains("request") || type.Contains("right"))
                    {
                        rights.Add(int.Parse(id));
                    }
                }
            }
            return (rights, roles);
        }

        public static (IEnumerable<UserRequestRight> rights, IEnumerable<UserITRole> roles) PreparePermissions
    ((IEnumerable<int> rightIds, IEnumerable<int> roleIds) ids, string login)
        {
            var userRightsList = ids.rightIds
                    .Select(x => new UserRequestRight() { UserId = login, RightId = x });
            var userRolesList = ids.roleIds
                .Select(x => new UserITRole() { UserId = login, RoleId = x });
            return (userRightsList, userRolesList);
        }
    }
}
