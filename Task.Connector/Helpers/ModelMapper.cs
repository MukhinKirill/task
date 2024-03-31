using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Task.Connector.Extensions;
using Task.Integration.Data.DbCommon.DbModels;
using Task.Integration.Data.Models.Models;

namespace Task.Connector.Helpers
{
    public static class ModelMapper
    {
        //private static string? GetUserToCreatePropertyValue(UserToCreate user,string propertyName)
        //{
        //    return user.Properties?.FirstOrDefault(x => x.Name.Equals(propertyName))?.Value;
        //}
        
        public static User Map(UserToCreate userToCreate)
        {
            var user = new User();
            user.ChangeProperties(userToCreate.Properties);
            user.Login = userToCreate.Login;
            return user;
        }
        public static IEnumerable<Permission> Map(IEnumerable<RequestRight> requestRights)
        {
            var permissions = new List<Permission>();
            foreach (var requestRight in requestRights)
            {
                if (requestRight is null)
                    continue;
                permissions.Add(new(requestRight.Id!.ToString()!, requestRight.Name, ""));
            }
            return permissions;
        }
        public static IEnumerable<Permission> Map(IEnumerable<ITRole> itRoles)
        {
            var permissions = new List<Permission>();
            foreach (var itRole in itRoles)
            {
                if (itRole is null)
                    continue;
                permissions.Add(new(itRole.Id!.ToString()!, itRole.Name, ""));
            }
            return permissions;
        }
    }
}
