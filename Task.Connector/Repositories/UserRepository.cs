using Task.Connector.Repositories.Interfaces;
using Task.Integration.Data.DbCommon;
using Task.Integration.Data.DbCommon.DbModels;
using Task.Integration.Data.Models.Models;

namespace Task.Connector.Repositories;

public class UserRepository : IUserRepository
{
    private readonly DataContext _dbContext; 

    public UserRepository(string connectionString)
    {
        var dbContextFactory = new DbContextFactory(connectionString);
        _dbContext = dbContextFactory.GetContext("POSTGRE");
    }

    public void CreateUser(User newUser)
    {
        _dbContext.Users.Add(newUser);
        _dbContext.SaveChanges();
    }

    public bool IsUserExists(string userLogin)
    {
        return _dbContext.Users.Any(u => u.Login == userLogin);
    }

    public User GetUserByLogin(string userLogin)
    {
        return _dbContext.Users.FirstOrDefault(u => u.Login == userLogin);
    }

    public void UpdateUserProperties(User user)
    {
        _dbContext.Users.Update(user);
        _dbContext.SaveChanges();
    }
}