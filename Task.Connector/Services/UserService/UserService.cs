using Task.Integration.Data.DbCommon;
using Task.Integration.Data.Models.Models;

namespace Task.Connector.Services.UserService;

internal class UserService : IUserService
{
    private DbContextFactory _contextFactory;
    private string _provider;

    internal UserService(DbContextFactory contextFactory, string provider)
    {
        _contextFactory = contextFactory;
        _provider = provider;
    }

    public bool IsUserExists(string userLogin)
    {
        using var context = _contextFactory.GetContext(_provider);

        var user = context.Users.FirstOrDefault(u => u.Login == userLogin);

        return user != null;
    }

    public void CreateUser(UserToCreate user)
    {
        using var context = _contextFactory.GetContext(_provider);

        context.Users.Add(new()
        {
            Login = user.Login,
            LastName = GetStringProperyOrDefault("lastName"),
            FirstName = GetStringProperyOrDefault("firstName"),
            MiddleName = GetStringProperyOrDefault("middleName"),
            TelephoneNumber = GetStringProperyOrDefault("telephoneNumber"),
            IsLead = GetBoolPropertyOrDefault("isLead")
        });

        context.Passwords.Add(new()
        {
            UserId = user.Login,
            Password = user.HashPassword
        });

        context.SaveChanges();


        UserProperty? GetProperty(string title)
        {
            return user.Properties.FirstOrDefault(up => up.Name == title);
        }

        string GetStringProperyOrDefault(string title)
        {
            var property = GetProperty(title);
            return property is null || property.Value is null ? "" : property.Value;
        }

        bool GetBoolPropertyOrDefault(string title)
        {
            var property = GetProperty(title);

            if (bool.TryParse(property?.Value, out var res))
            {
                return res;
            }

            return false;
        }
    }
}
