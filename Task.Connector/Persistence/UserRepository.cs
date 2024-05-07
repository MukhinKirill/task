using System.Transactions;

using Task.Connector.Entities;
using Task.Integration.Data.DbCommon;
using Task.Integration.Data.DbCommon.DbModels;

namespace Task.Connector.Persistence;
public sealed class UserRepository
{
    private readonly DataContext _context;

    public UserRepository(DataContext context)
    {
        _context = context;
    }

    public int GetCountUsers()
    {
        return _context.Users.Count();
    }

    public void Create(UserModel userModel)
    {
        if (userModel == null)
        {
            throw new ArgumentNullException(nameof(userModel));
        }

        using var scope = new TransactionScope(TransactionScopeOption.Required);
        _context.Users.Add(new User()
        {
            Login = userModel.Login,
            FirstName = userModel.FirstName,
            LastName = userModel.LastName,
            MiddleName = userModel.MiddleName,
            TelephoneNumber = userModel.TelephoneNumber,
            IsLead = userModel.IsLead,
        });

        _context.Passwords.Add(new()
        {
            Password = userModel.Password,
            UserId = userModel.Login
        });

        _context.SaveChanges();

        scope.Complete();
    }
}
