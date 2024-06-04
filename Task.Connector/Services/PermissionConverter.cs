using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Task.Connector.Models;
using Task.Integration.Data.Models.Models;

namespace Task.Connector.Services
{
    public class PermissionConverter
    {
        public List<Permission> GetAllPermissionFrom(IEnumerable<ItRole> roles, IEnumerable<RequestRight> rights)
        {
            var allPermissions = new List<Permission>();
            foreach (var role in roles)
            {
                allPermissions.Add(new Permission(role.Id.ToString(), role.Name, $"Role"));
            }
            foreach (var right in rights)
            {
                allPermissions.Add(new Permission(right.Id.ToString(), right.Name, $"Request"));
            }
            return allPermissions;
        }

        public (List<ItRole> roles, List<RequestRight> rights) SortPermissonsToBase(List<string> ids)
        {
            return (null, null);
        }

    }
}
