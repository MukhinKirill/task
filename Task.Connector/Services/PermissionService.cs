using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Task.Integration.Data.DbCommon;
using Task.Integration.Data.Models.Models;

namespace Task.Connector.Services
{
    public interface IPermissionService
    {
        IEnumerable<Permission> GetAllPermissions();
    }
    internal class PermissionService : BaseService, IPermissionService
    {
        public PermissionService(DataContext context) : base(context) { }

        public IEnumerable<Permission> GetAllPermissions()
        {
            var rigths = Context.RequestRights.Select(x => new Permission(x.Id.ToString(), x.Name, "{\"permissionType\": \"RequestRight\"}")).ToList();
            var roles = Context.ITRoles.Select(x => new Permission(x.Id.ToString(), x.Name, $"{{\"corporatePhone\": \"{x.CorporatePhoneNumber}\", \"permissionType\": \"ITRole\"}}")).ToList();
            return rigths.Concat(roles);
        }
    }
}
