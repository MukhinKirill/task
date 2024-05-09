using Task.Connector.Infrastructure.DataModels;

namespace Task.Connector.Infrastructure.Repository.Interfaces;

public interface IUserRepository
{
    public void Create(UserDataModel user);
    public bool IsExists(string login);
    public UserDataModel? GetUserModelByLogin(string login);
    public void Update(UserDataModel user);
}