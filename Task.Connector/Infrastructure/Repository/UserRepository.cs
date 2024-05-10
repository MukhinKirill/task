using Task.Connector.Domain.Models;
using Task.Connector.Infrastructure.Context;
using Task.Connector.Infrastructure.DataModels;
using Task.Connector.Infrastructure.Repository.Interfaces;
using Task.Integration.Data.Models;

namespace Task.Connector.Infrastructure.Repository;

public sealed class UserRepository : IUserRepository
{
    private readonly AvanpostContext _context;
    private readonly ILogger _logger;

    public UserRepository(AvanpostContext context, ILogger logger)
    {
        _context = context;
        _logger = logger;
    }

    public void Create(UserDataModel user)
    {
        using var transaction = _context.Database.BeginTransaction();
        try
        {
            _context.Users.Add(new User
            {
                Login = user.Login,
                FirstName = user.FirstName,
                LastName = user.LastName,
                MiddleName = user.MiddleName,
                TelephoneNumber = user.TelephoneNumber,
                IsLead = user.IsLead,
            });

            _context.Passwords.Add(new Sequrity
            {
                UserId = user.Login,
                Password = user.Password
            });
                
            _context.SaveChanges();
            transaction.Commit();
        }
        catch (Exception)
        {
            transaction.Rollback();
            _logger.Error($"Invalid user {user.Login}");
        }
    }

    public bool IsExists(string login) => _context.Users.Any(u => u.Login == login);
    
    public UserDataModel? GetUserModelByLogin(string login)
    {
        return _context.Users
            .Where(user => user.Login == login)
            .Join(_context.Passwords, user => user.Login, password => password.UserId,
                (user, password) => new UserDataModel
                {
                    Login = user.Login,
                    FirstName = user.FirstName,
                    MiddleName = user.MiddleName,
                    LastName = user.LastName,
                    IsLead = user.IsLead,
                    Password = password.Password,
                    TelephoneNumber = user.TelephoneNumber
                })
            .SingleOrDefault();
    }

    public void Update(UserDataModel user)
    {
        using var transaction = _context.Database.BeginTransaction();
        try
        {
            _context.Users.Update(new User
            {
                Login = user.Login,
                FirstName = user.FirstName,
                LastName = user.LastName,
                MiddleName = user.MiddleName,
                TelephoneNumber = user.TelephoneNumber,
                IsLead = user.IsLead,
            });

            _context.Passwords.Update(new Sequrity
            {
                UserId = user.Login,
                Password = user.Password
            });
                
            _context.SaveChanges();
            transaction.Commit();
        }
        catch (Exception)
        {
            transaction.Rollback();
            _logger.Error($"Invalid user {user.Login}");
        }
    }
}