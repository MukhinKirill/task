using Microsoft.EntityFrameworkCore;
using Task.Connector.Models;
using Task.Integration.Data.Models;
using Task.Integration.Data.Models.Models;

namespace Task.Connector
{
    public class ConnectorDb : IConnector
    {
        private TestTaskDbContext? _context;

        public void StartUp(string connectionString)
        {
            var options = new DbContextOptionsBuilder<TestTaskDbContext>()
                .UseNpgsql(connectionString)
                .Options;

            _context = new TestTaskDbContext(options);
        }

        public void CreateUser(UserToCreate user)
        {
            if (user == null)
            {
                Logger.Error("CreateUser was called with a null argument.");
                throw new ArgumentNullException(nameof(user));
            }

            // Проверка на существование пользователя с таким же логином
            if (_context != null && _context.Users.Any(u => u.Login == user.Login))
            {
                Logger.Error("User with the specified login already exists.");
                throw new InvalidOperationException("User with this login already exists.");
            }

            // Получаем значения свойств с обработкой возможных null-значений
            string lastName = user.Properties.FirstOrDefault(p => p.Name == "lastName")?.Value ?? string.Empty;
            string firstName = user.Properties.FirstOrDefault(p => p.Name == "firstName")?.Value ?? string.Empty;
            string middleName = user.Properties.FirstOrDefault(p => p.Name == "middleName")?.Value ?? string.Empty;
            string telephoneNumber = user.Properties.FirstOrDefault(p => p.Name == "telephoneNumber")?.Value ?? string.Empty;
            bool isLead = bool.TryParse(user.Properties.FirstOrDefault(p => p.Name == "isLead")?.Value, out bool parsedIsLead) && parsedIsLead;

            // Создаем нового пользователя
            var newUser = new User
            {
                Login = user.Login,
                LastName = lastName,
                FirstName = firstName,
                MiddleName = middleName,
                TelephoneNumber = telephoneNumber,
                IsLead = isLead
            };

            // Добавляем нового пользователя в контекст
            _context?.Users.Add(newUser);

            // Создаем запись для пароля пользователя
            var passwordEntry = new Password
            {
                UserId = user.Login,
                Password1 = user.HashPassword
            };

            _context?.Passwords.Add(passwordEntry);

            // Сохраняем изменения в базе данных
            _context?.SaveChanges();
        }

        public IEnumerable<Property> GetAllProperties()
        {
            throw new NotImplementedException();
        }

        public IEnumerable<UserProperty> GetUserProperties(string userLogin)
        {
            throw new NotImplementedException();
        }

        public bool IsUserExists(string userLogin)
        {
            throw new NotImplementedException();
        }

        public void UpdateUserProperties(IEnumerable<UserProperty> properties, string userLogin)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<Permission> GetAllPermissions()
        {
            throw new NotImplementedException();
        }

        public void AddUserPermissions(string userLogin, IEnumerable<string> rightIds)
        {
            throw new NotImplementedException();
        }

        public void RemoveUserPermissions(string userLogin, IEnumerable<string> rightIds)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<string> GetUserPermissions(string userLogin)
        {
            throw new NotImplementedException();
        }

        public ILogger Logger { get; set; }
    }
}