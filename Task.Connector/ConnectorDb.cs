using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;
using Task.Connector.ContextConstruction;
using Task.Connector.ContextConstruction.ContextFactory;
using Task.Connector.ContextConstruction.Converter;
using Task.Connector.ContextConstruction.PermissionContext;
using Task.Connector.ContextConstruction.UserContext;
using Task.Connector.Models.ModelConversion;
using Task.Connector.Models.Schemas;
using Task.Connector.RequestHandling;
using Task.Integration.Data.Models;
using Task.Integration.Data.Models.Models;

namespace Task.Connector
{
    public class ConnectorDb : IConnector
    {
        private readonly UserSchema _userSchema;
        private readonly PermissionSchema[] _permissionSchemas;
        private IDynamicContextFactory<DynamicUserContext>? _userContextFactory;
        private IDynamicContextFactory<DynamicPermissionContext>[]? _permissionContextFactories;
        private readonly IModelGenerator<DynamicUserContext> _userModelGenerator;
        private readonly IModelGenerator<DynamicPermissionContext>[] _permissionModelGenerators;
        private readonly IRawUserRequestHandler _userRequestHandler;
        private IUserConverter _userConverter;
        private readonly IPermissionRequestHandler _permissionRequestHandler;

        // В теории можно передать схемы, не соответствующие генераторам моделей,
        // однако, чтобы это исправить, придется ввести фабричные методы,
        // что я счёл излишним
        public ConnectorDb(UserSchema? userSchema = null,
                           PermissionSchema[]? permissionSchemas = null,
                           IModelGenerator<DynamicUserContext>? userModelGenerator = null,
                           IModelGenerator<DynamicPermissionContext>[]? permissionModelGenerators = null,
                           IRawUserRequestHandler? userRequestHandler = null,
                           IUserConverter? userConverter = null,
                           IPermissionRequestHandler? permissionRequestHandler = null)
        {
            _userSchema = userSchema ?? _defaultUserSchema;
            _permissionSchemas = permissionSchemas ?? _defaultPermissionSchemas;
            _userModelGenerator = userModelGenerator ?? _defaultUserModelGenerator;
            _permissionModelGenerators = permissionModelGenerators ?? _defaultPermissionModelGenerators;
            _userRequestHandler = userRequestHandler ?? _defaultUserRequestHandler;
            _userConverter = userConverter ?? _defaultUserConverter;
            _permissionRequestHandler = permissionRequestHandler ?? _defaultPermissionRequestHandler;
        }

        private (string dbConnectionString, string schemaName) ConvertConnectionString(string connectionString)
        {
            string pattern = @"ConnectionString='(.+)';Provider=.+;SchemaName='(.+)'";
            var match = Regex.Match(connectionString, pattern);
            return (match.Groups[1].Value, match.Groups[2].Value);
        }

        public void StartUp(string connectionString)
        {
            (string dbConnectionString, string schemaName) = ConvertConnectionString(connectionString);

            var userContextBuilder = new DbContextOptionsBuilder<DynamicUserContext>();
            var permissionContextBuilder = new DbContextOptionsBuilder<DynamicPermissionContext>();

            // Мне не удалось установить схему через параметр Search Path,
            // так как он не позволял работать с символами верхнего регистра
            // даже при обертке имени схемы в двойные кавычки
            userContextBuilder.UseNpgsql(dbConnectionString);
            permissionContextBuilder.UseNpgsql(dbConnectionString);

            _userContextFactory = new PooledDynamicContextFactory<DynamicUserContext>(_userModelGenerator, userContextBuilder, schemaName);
            _permissionContextFactories = (from generator in _permissionModelGenerators
                                           select new PooledDynamicContextFactory<DynamicPermissionContext>
                                           (generator, permissionContextBuilder, schemaName)).ToArray();

            _userRequestHandler.Initialize(_userContextFactory);
            _permissionRequestHandler.Initialize(_permissionContextFactories, schemaName);     
        }

        public void CreateUser(UserToCreate user)
        {
            _userRequestHandler.CreateUser(_userConverter.ConvertUserToCreate(user));
        }

        public IEnumerable<Property> GetAllProperties()
        {
            return _userConverter.ConvertProperties(_userRequestHandler.GetAllProperties());
        }

        public IEnumerable<UserProperty> GetUserProperties(string userLogin)
        {
            return _userConverter.ExtractProperties(_userRequestHandler.GetUserProperties(userLogin));
        }

        public bool IsUserExists(string userLogin)
        {
            return _userRequestHandler.IsUserExists(userLogin);
        }

        public void UpdateUserProperties(IEnumerable<UserProperty> properties, string userLogin)
        {
            _userRequestHandler.UpdateUserProperties(_userConverter.ConstructUser(properties, userLogin));
        }

        public IEnumerable<Permission> GetAllPermissions()
        {
            return _permissionRequestHandler.GetAllPermissions();
        }

        public void AddUserPermissions(string userLogin, IEnumerable<string> rightIds)
        {
            _permissionRequestHandler.AddUserPermissions(userLogin, rightIds);
        }

        public void RemoveUserPermissions(string userLogin, IEnumerable<string> rightIds)
        {
            _permissionRequestHandler.RemoveUserPermissions(userLogin, rightIds);
        }

        public IEnumerable<string> GetUserPermissions(string userLogin)
        {
            return _permissionRequestHandler.GetUserPermissions(userLogin);
        }

        public ILogger Logger { get; set; }

        // Так как в задаче не подразумевается использование Dependency Injection,
        // то настраивать зависимости придётся самостоятельно

        private static readonly UserSchema _defaultUserSchema = new UserSchema("User", "Passwords", 
            new Dictionary<string, string> {
                { "lastName", "string" },
                { "firstName", "string"},
                { "middleName", "string"},
                { "telephoneNumber", "string" },
                { "isLead", "boolean" } });

        private static readonly PermissionSchema[] _defaultPermissionSchemas = new PermissionSchema[] {
            new PermissionSchema("ItRole", "UserITRole", true, "roleId", "Role", ":", "corporatePhoneNumber"),
            new PermissionSchema("RequestRight", "UserRequestRight", false, "rightId", "Request", ":")
        };

        private static readonly IConverter _defaultConverter = new DefaultConverter();

        private static readonly IModelGenerator<DynamicUserContext> _defaultUserModelGenerator =
            new UserModelGenerator(_defaultUserSchema, _defaultConverter);

        private static readonly IModelGenerator<DynamicPermissionContext>[] _defaultPermissionModelGenerators =
            (from schema in _defaultPermissionSchemas
             select new PermissionModelGenerator(schema, _defaultConverter)).ToArray();

        private static readonly IRawUserRequestHandler _defaultUserRequestHandler = new RawUserRequestHandler(_defaultUserSchema.PropertyTypes);

        private static readonly IUserConverter _defaultUserConverter = new UserConverter();

        private static readonly IPermissionRequestHandler _defaultPermissionRequestHandler = new PermissionRequestHandler(_defaultPermissionSchemas);
    }
}