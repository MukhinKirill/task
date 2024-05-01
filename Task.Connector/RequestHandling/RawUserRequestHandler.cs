using Task.Connector.ContextConstruction.ContextFactory;
using Task.Connector.ContextConstruction.UserContext;

using UserObj = System.Collections.Generic.Dictionary<string, object>;
using Properties = System.Collections.Generic.Dictionary<string, string>;

namespace Task.Connector.RequestHandling
{
    public class RawUserRequestHandler : IRawUserRequestHandler
    {
        private IDynamicContextFactory<DynamicUserContext>? _contextFactory;
        private Properties _properties;
        private bool _isInitialized = false;

        public RawUserRequestHandler(Properties properties)
        {
            this._properties = properties;
            _properties["password"] = "string";
        }

        public void Initialize(IDynamicContextFactory<DynamicUserContext> contextFactory)
        {
            _contextFactory = contextFactory;
            _isInitialized = true;
        }

        public void CreateUser(UserObj user)
        {
            if (!_isInitialized)
            {
                throw new InvalidOperationException("Метод CreateUser вызван до инициализации обработчика");
            }

            using var context = _contextFactory!.CreateContext();
            context.Users.Add(user);
            context.SaveChanges();
        }

        public Properties GetAllProperties()
        {
            if (!_isInitialized)
            {
                throw new InvalidOperationException("Метод GetAllProperties вызван до инициализации обработчика");
            }

            return _properties;
        }

        public UserObj GetUserProperties(string userLogin)
        {
            if (!_isInitialized)
            {
                throw new InvalidOperationException("Метод GetUserProperties вызван до инициализации обработчика");
            }

            using var context = _contextFactory!.CreateContext();
            var user = context.Users.Where(user => user["login"].Equals(userLogin)).First();

            // Я не знаю, как составить проекцию Select для property bag,
            // чтобы запрашивать меньше свойств сущности
            user.Remove("login");

            return user;
        }

        public bool IsUserExists(string userLogin)
        {
            if (!_isInitialized)
            {
                throw new InvalidOperationException("Метод IsUserExists вызван до инициализации обработчика");
            }

            using var context = _contextFactory!.CreateContext();
            var userExists = context.Users.Where(user => user["login"].Equals(userLogin)).Any();

            return userExists;
        }

        public void UpdateUserProperties(UserObj user)
        {
            if (!_isInitialized)
            {
                throw new InvalidOperationException("Метод UpdateUserPermission вызван до инициализации обработчика");
            }

            using var context = _contextFactory!.CreateContext();
            var entry = context.Users.Update(user);

            // Необходимо пометить, какие из полей обновлены,
            // а какие - нет

            foreach (var property in _properties)
            {
                entry.Property(property.Key).IsModified = false;
            }

            foreach (var property in user)
            {
                if (property.Key != "login")
                {
                    entry.Property(property.Key).IsModified = true;
                }
                
            }

            context.SaveChanges();
        }
    }
}
