using Connector.Infrastructure.DataAccess.Models.POCO;
using Task.Integration.Data.Models.Models;

namespace Connector.Infrastructure.DataAccess.Maps
{
    public static class RolePocoToPermission
    {
        private const string groupName = "Role";
        private const string delimeter = ":";

        public static IEnumerable<Permission> Convert(this IEnumerable<RolePOCO> roles)
        {
            foreach (var role in roles)
            {
                yield return new Permission(
                    GetId(role.Id),
                    role.Name,
                    role.CorporatePhoneNumber);
            }
        }

        public static IEnumerable<string> ConvertRoleIds(this IEnumerable<int> ids)
        {
            foreach (var id in ids)
            {
                yield return GetId(id);
            }
        }

        private static string GetId(int id)
        {
            return string.Join(delimeter, groupName, id);
        }
    }
}
