using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tasks=System.Threading.Tasks;
using Task.Integration.Data.Models.Models;

namespace Task.Connector.Abstractions
{
    internal interface IAvanpostRepository
    {
        void StartUp(string connectionString);

        bool CreateUser(UserToCreate user);

        IEnumerable<Property> GetAllProperties();

        IEnumerable<UserProperty> GetUserProperties(string userLogin);

        bool IsUserExists(string userLogin);

        bool UpdateUserProperties(IEnumerable<UserProperty> properties, string userLogin);

        IEnumerable<Permission> GetAllPermissions();

        bool AddUserPermissions(string userLogin, IEnumerable<string> rightIds);

        bool RemoveUserPermissions(string userLogin, IEnumerable<string> rightIds);

        IEnumerable<string> GetUserPermissions(string userLogin);
    }
}
