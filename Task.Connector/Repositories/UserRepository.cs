using Task.Connector.Interfaces;
using Task.Connector.Mapper;
using Task.Connector.Utils;
using Task.Integration.Data.DbCommon;
using Task.Integration.Data.DbCommon.DbModels;
using Task.Integration.Data.Models;
using Task.Integration.Data.Models.Models;

namespace Task.Connector.Repositories;

internal class UserRepository : IUserRepository
{
    private readonly ILogger _logger;
    private readonly DataContext _context;

    //Можно заменить на json файл и брать строки из него. Специально статика, чтобы не выделялось место под эти данные в каждом экземляре класса
    private static IDictionary<string, string> _userPropertyDescriptions = new Dictionary<string, string>()
            {
                {"LastName", "Users name" },
                {"FirstName", "Users first name" },
                {"MiddleName", "Users middle name" },
                {"TelephoneNumber", "Users phone number" },
                {"IsLead", "Is User a lead" },
                {"Password", "Users Password" }
            };

    public UserRepository(ILogger logger, DataContext context)
    {
        _logger = logger;
        _context = context;
    }

    public void CreateUser(UserToCreate user)
    {
        if (!IsUserExists(user.Login))
            throw new InvalidOperationException("The user with login {user.Login} is exist!");

        var entity = UserMapper.CreateUserFromUserProps(user);

        _context.Users.Add(entity);
        _context.Passwords.Add(
            new()
            {
                UserId = entity.Login,
                Password = user.HashPassword
            }
        );

        _context.SaveChanges();
    }

    public IEnumerable<Property> GetAllProperties()
    {
        var props = new List<Property>();
        var propsNames = typeof(User).GetProperties().Select(p => p.Name).ToList();
        propsNames.Add("Password");
        foreach (var name in propsNames)
        {
            if (name == "Login") continue;

            if (!_userPropertyDescriptions.TryGetValue(name, out var description))
            {
                _logger.Warn($"No description for the property {name}");
                description = string.Empty;
            }

            props.Add(new(name, description));
        }
        return props;
    }

    public IEnumerable<UserProperty> GetUserProperties(string userLogin)
    {
        var user = _context.Users.FirstOrDefault(x => x.Login == userLogin);
        return user == null
            ? throw new InvalidOperationException($"The user with login {userLogin} does not exist!")
            : typeof(User).GetProperties().Where(p => p.Name != "Login").Select(p => new UserProperty(p.Name, p.GetValue(user).ToString()));
    }

    public bool IsUserExists(string userLogin)
    {
        return _context.Users.Any(u => u.Login == userLogin);
    }

    public void UpdateUserProperties(IEnumerable<UserProperty> properties, string userLogin)
    {
        var user = _context.Users.FirstOrDefault(u => u.Login == userLogin) ?? 
            throw new InvalidOperationException($"The user with login {userLogin} does not exist!");

        foreach (var property in properties)
        {
            if (user.WithProperty(property.Name, property.Value))
                _logger.Warn($"The property with name {property.Name} does not exist!");
        }
        _context.SaveChanges();
    }
}
