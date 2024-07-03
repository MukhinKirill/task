using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Task.Integration.Data.Models.Models;

namespace Task.Connector.Services.Interfaces
{
    public interface IPermissionService
    {
        public IEnumerable<Permission> GetAllPermissions();
        public void AddUserPermissions(string userLogin, IEnumerable<string> rightIds);
        public void RemoveUserPermissions(string userLogin, IEnumerable<string> rightIds);
        public IEnumerable<string> GetUserPermissions(string userLogin);
    }
}
