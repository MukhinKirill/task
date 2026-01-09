using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TR.Connector
{
    internal static class ApiEndpoints
    {
        public const string Login = "api/v1/login";
        public const string RolesAll = "api/v1/roles/all";
        public const string RightsAll = "api/v1/rights/all";
        public const string UsersAll = "api/v1/users/all";
        public const string EditUser = "api/v1/users/edit";
        public const string CreateUser = "api/v1/users/create";

        
        private const string UserTemplate = "api/v1/users/{0}";
        private const string UserRolesTemplate = "api/v1/users/{0}/roles";
        private const string UserRightsTemplate = "api/v1/users/{0}/rights";
        private const string AddRoleTemplate = "api/v1/users/{0}/add/role/{1}";
        private const string AddRightTemplate = "api/v1/users/{0}/add/right/{1}";
        private const string DropRoleTemplate = "api/v1/users/{0}/drop/role/{1}";
        private const string DropRightTemplate = "api/v1/users/{0}/drop/right/{1}";
        private const string LockUserTemplate = "api/v1/users/{0}/lock";
        private const string UnlockUserTemplate = "api/v1/users/{0}/unlock";
        public static string UserByLogin(string login) => string.Format(UserTemplate, Uri.EscapeDataString(login));
        public static string UserRoles(string login) => string.Format(UserRolesTemplate, Uri.EscapeDataString(login));
        public static string UserRights(string login) => string.Format(UserRightsTemplate, Uri.EscapeDataString(login));
        public static string AddRole(string login, int roleId) => string.Format(AddRoleTemplate, Uri.EscapeDataString(login), roleId);
        public static string AddRight(string login, int rightId) => string.Format(AddRightTemplate, Uri.EscapeDataString(login), rightId);
        public static string DropRole(string login, int roleId) => string.Format(DropRoleTemplate, Uri.EscapeDataString(login), roleId);
        public static string DropRight(string login, int rightId) => string.Format(DropRightTemplate, Uri.EscapeDataString(login), rightId);
        public static string LockUser(string login) => string.Format(LockUserTemplate, Uri.EscapeDataString(login));
        public static string UnlockUser(string login) => string.Format(UnlockUserTemplate, Uri.EscapeDataString(login));

    }
}
