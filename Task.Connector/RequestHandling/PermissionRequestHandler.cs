using Microsoft.EntityFrameworkCore;
using System.Text;
using System.Text.RegularExpressions;
using Task.Connector.ContextConstruction.ContextFactory;
using Task.Connector.ContextConstruction.PermissionContext;
using Task.Connector.Models.Schemas;
using Task.Integration.Data.Models.Models;

namespace Task.Connector.RequestHandling
{
    public class PermissionRequestHandler : IPermissionRequestHandler
    {
        private IDynamicContextFactory<DynamicPermissionContext>[]? _contextFactories;
        private PermissionSchema[] _permissionSchemas;
        private bool _isInitialized = false;
        private string? _schemaName;

        public PermissionRequestHandler(PermissionSchema[] permissionSchemas)
        {
            _permissionSchemas = permissionSchemas;
        }

        // Каждый элемент contextFactories должен генерировать контексты, соответствующие permissionSchemas по тому же индексу
        public void Initialize(IDynamicContextFactory<DynamicPermissionContext>[] contextFactories, string schemaName)
        {
            _contextFactories = contextFactories;
            _schemaName = schemaName;
            _isInitialized = true;
        }


        // Данный метод применяет некоторую SQL-операцию
        // ко всем имеющимся у пользователя правам.

        // Сам SQL-код для конкретного rightId генерируется в commandBuilder
        // Весь SQL-код складывается в одну команду к БД

        // В теории можно обернуть разные SQL-запросы в одну транзакцию        
        private void ExecuteUserPermissionsOperation(string userLogin, IEnumerable<string> rightIds, Func<PermissionSchema, int, string> commandBuilder)
        {
            var valuesByGroupName = new Dictionary<string, List<int>>();
            foreach (var schema in _permissionSchemas)
            {
                valuesByGroupName[schema.GroupName] = new List<int>();
            }

            var format = @"(.+):(.+)";
            foreach (var rightId in rightIds)
            {
                var split = Regex.Match(rightId, format);
                var groupName = split.Groups[1].Value;
                var id = int.Parse(split.Groups[2].Value);
                valuesByGroupName[groupName].Add(id);
            }

            var sqlBuilder = new StringBuilder();

            foreach (var schema in _permissionSchemas!)
            {
                foreach (var value in valuesByGroupName[schema.GroupName])
                {
                    sqlBuilder.AppendLine(commandBuilder(schema, value));
                }
            }

            using var context = _contextFactories![0].CreateContext();
            context.Database.ExecuteSqlRaw(sqlBuilder.ToString(), new object[] { userLogin });
        }

        public void AddUserPermissions(string userLogin, IEnumerable<string> rightIds)
        {
            if (!_isInitialized)
            {
                throw new InvalidOperationException("Метод AddUserPermission вызван до инициализации обработчика");
            }

            var commandBuilder = (PermissionSchema schema, int value) =>
                                          @$"INSERT INTO ""{_schemaName}"".""{schema.UserPermissionTableName}""
                                             (""userId"", ""{schema.PermissionIdName}"")
                                             VALUES ((@p0), {value})";
            ExecuteUserPermissionsOperation(userLogin, rightIds, commandBuilder);   
        }

        public void RemoveUserPermissions(string userLogin, IEnumerable<string> rightIds)
        {
            if (!_isInitialized)
            {
                throw new InvalidOperationException("Метод RemoveUserPermissions вызван до инициализации обработчика");
            }

            var commandBuilder = (PermissionSchema schema, int value) =>
                                          @$"DELETE FROM ""{_schemaName}"".""{schema.UserPermissionTableName}""
                                             WHERE ""userId"" = (@p0) AND ""{schema.PermissionIdName}"" = {value};";

            ExecuteUserPermissionsOperation(userLogin, rightIds, commandBuilder);
        }

        // SqlQuery<> имеет возможность возвращать любые CLR-типы,
        // но только начиная с EF Core 8, поэтому выполнить
        // все запросы в одно обращение к БД не выйдет
        public IEnumerable<Permission> GetAllPermissions()
        {
            if (!_isInitialized)
            {
                throw new InvalidOperationException("Метод GetAllPermissions вызван до инициализации обработчика");
            }

            IEnumerable<Permission> permissions = new List<Permission>();

            foreach (var factory in _contextFactories!)
            {
                using var context = factory.CreateContext();

                permissions = permissions.Concat(context.PermissionTypes.Select(pType => pType.ToPermission()).ToList());
            }

            return permissions;
        }

        public IEnumerable<string> GetUserPermissions(string userLogin)
        {
            if (!_isInitialized)
            {
                throw new InvalidOperationException("Метод GetUserPermissions вызван до инициализации обработчика");
            }

            var sqlBuilder = new StringBuilder();

            // Данный код собирает SQL-запросы для получения прав пользователя
            // по каждой отдельной разновидности прав,
            // а затем собирает через UNION в один SQL-запрос к БД

            for (var i = 0; i < _permissionSchemas!.Length; i++)
            {
                var schema = _permissionSchemas[i];

                var prefix = schema.GroupName + schema.Delimeter;

                sqlBuilder.AppendLine(@$"SELECT CONCAT('{prefix}', ""{schema.PermissionIdName}"")
                                    FROM ""{_schemaName}"".""{schema.UserPermissionTableName}""
                                    WHERE ""userId"" = (@p0)");

                if (i < _permissionSchemas.Length - 1)
                {
                    sqlBuilder.AppendLine("UNION");
                }
                else
                {
                    sqlBuilder.Append(';');
                }
            }

            using var context = _contextFactories![0].CreateContext();
            return context.Database.SqlQueryRaw<string>(sqlBuilder.ToString(), new object[] { userLogin }).ToList();
        }
    }
}
