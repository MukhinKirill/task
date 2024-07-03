using Task.Integration.Data.Models.Models;

namespace Task.Connector.Services.Interfaces
{
    public interface IUserService
    {
        public bool IsUserExists(string userLogin);
        public void CreateUser(UserToCreate user);
    }
}
