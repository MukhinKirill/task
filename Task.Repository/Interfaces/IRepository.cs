using Task.Integration.Data.DbCommon.DbModels;
using Task.Integration.Data.Models.Models;

namespace Task.Repository.Interfaces
{
    public interface IRepository
    {
        void CreateUser(User user);

        IEnumerable<UserProperty> GetUserProperties(string userLogin);

        bool IsUserExists(string userLogin);

        void UpdateUserProperties(IEnumerable<UserProperty> properties, string userLogin);

        IEnumerable<Permission> GetAllPermissions();

        void AddUserPermissions(string userLogin, IEnumerable<string> rightIds);

        void RemoveUserPermissions(string userLogin, IEnumerable<string> rightIds);

        IEnumerable<string> GetUserPermissions(string userLogin);
    }
}
