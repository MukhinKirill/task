using Microsoft.Identity.Client.Extensions.Msal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Task.Connector.Models;
using Task.Connector.Repositories;
using Task.Integration.Data.Models.Models;

namespace Task.Connector.Services
{
    public class PermissionConverter
    {
        private const string ROLE = "Role";
        private const string REQUEST = "Request";

        public List<Permission> GetAllPermissionFrom(IEnumerable<ItRole> roles, IEnumerable<RequestRight> rights)
        {
            var allPermissions = new List<Permission>();
            foreach (var role in roles)
            {
                allPermissions.Add(new Permission(role.Id.ToString(), role.Name, ROLE));
            }
            foreach (var right in rights)
            {
                allPermissions.Add(new Permission(right.Id.ToString(), right.Name, REQUEST));
            }
            return allPermissions;
        }

        public (List<UserItrole> userItRole, List<UserRequestRight> userRequestRights) SortPermissonsToData(string userLogin, IEnumerable<string> ids)
        {
            var userItRoles = new List<UserItrole>();
            var userRequestRights = new List<UserRequestRight>();
            foreach (var idString in ids)
            {
                var splitString = idString.Split(":");
                if (splitString[0].Equals(ROLE))
                {
                    var id = int.Parse(splitString[1]);
                    var userItRole = new UserItrole()
                    {
                        RoleId = id,
                        UserId = userLogin
                    };
                    userItRoles.Add(userItRole);
                }
                else if (splitString[0].Equals(REQUEST))
                {
                    var id = int.Parse(splitString[1]);
                    var userRequestRight = new UserRequestRight()
                    {
                        RightId = id,
                        UserId = userLogin
                    };
                    userRequestRights.Add(userRequestRight);
                } else
                {
                    throw new Exception($"Incorrect string {idString}.");
                }
            }
            return (userItRoles, userRequestRights);
        }

    }
}
