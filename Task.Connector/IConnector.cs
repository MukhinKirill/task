using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Task.Integration.Data.Models.Models;

namespace Task.Connector
{
    public interface IConnector
    {
        void StartUp(string connectionString);
        void CreateUser(UserToCreate user);
        bool IsUserExists(string userLogin);
        IEnumerable<Property> GetAllProperties();
        IEnumerable<UserProperty> GetUserProperties(string userLogin);
        void UpdateUserProperties(IEnumerable<UserProperty> properties, string userLogin);
    }
}
