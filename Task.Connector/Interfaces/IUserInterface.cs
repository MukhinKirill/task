using System.Collections;
using Task.Connector.Entities;
using Task.Integration.Data.Models.Models;

namespace Task.Connector.Interfaces;

public interface IUserInterface
{
    void CreateUser(UserToCreate userToCreate);
    
    void UpdateUserProperties(IEnumerable<UserProperty> properties, string userLogin);

    bool IsUserExists(string userLogin);
    
    User GetUserByLogin(string userLogin);

    IEnumerable<Property> GetAllProperties();

    IEnumerable<UserProperty> GetUserProperties(string userLogin);
}