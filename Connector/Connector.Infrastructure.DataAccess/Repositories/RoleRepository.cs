using Base.Models.Results;
using Connector.Core.Interfaces.DataAccess.Repositories;
using Connector.Infrastructure.DataAccess.Maps;
using Connector.Infrastructure.DataAccess.Models.POCO;
using Connector.Infrastructure.DataAccess.Utils;
using Dapper;
using System.Data;
using Task.Integration.Data.Models.Models;
using ILogger = Task.Integration.Data.Models.ILogger;

namespace Connector.Infrastructure.DataAccess.Repositories
{
    public class RoleRepository : IRoleRepository
    {
        #region Private

        private readonly IDbConnection _connection;
        private readonly IDbTransaction _transaction;
        private readonly string _schema;
        private readonly int _timeOut;
        private readonly ILogger _logger;

        #endregion

        public RoleRepository(IDbConnection connection,
            IDbTransaction transaction,
            string schema,
            int timeOut,
            ILogger logger)
        {
            _connection = connection;
            _transaction = transaction;
            _schema = schema;
            _timeOut = timeOut;
            _logger = logger;
        }

        #region Methods

        public Result AddUserRoles(string userLogin, IEnumerable<int> roleIds)
        {
            _logger.Debug($"Начало добавления ролей для пользователя {userLogin}");

            var filter = roleIds.FilterToDataAccess();

            var parameters = new DynamicParameters();
            parameters.Add("login", userLogin);
            parameters.Add(filter.name, filter.value);


            var roleQuery = @$"INSERT INTO ""{_schema}"".""UserITRole"" (""userId"", ""roleId"")
                               SELECT @login,
                                      id
                               FROM ""{_schema}"".""ItRole""
                               WHERE id = {filter.rowFilter}";

            try
            {
                _connection.Execute(roleQuery,
                    param: roleIds.Select(a => new { login = userLogin, ids = a }),
                    transaction: _transaction,
                    commandTimeout: _timeOut);

                _logger.Debug($"Успешное добавление ролей для пользователя {userLogin}");

                return Result.CreateSuccessResult();
            }
            catch (Exception ex)
            {
                _logger.Error($"Исключение при добавлении ролей для пользователя {userLogin}. Ex = {ex.Message}");

                return Result.CreateErrorResult(ex.Message);
            }
        }

        public Result DeleteUserRoles(string userLogin, IEnumerable<int> roleIds)
        {
            _logger.Debug($"Начало удаления ролей для пользователя {userLogin}");

            var filter = roleIds.FilterToDataAccess();

            var parameters = new DynamicParameters();
            parameters.Add("login", userLogin);
            parameters.Add(filter.name, filter.value);

            var roleQuery = $@"DELETE
                               FROM ""{_schema}"".""UserITRole""
                               WHERE ""userId"" = @login AND
                                     ""roleId"" = {filter.rowFilter}";

            try
            {
                _connection.Execute(roleQuery,
                    param: parameters,
                    transaction: _transaction,
                    commandTimeout: _timeOut);

                _logger.Debug($"Успешное удаление ролей для пользователя {userLogin}");

                return Result.CreateSuccessResult();
            }
            catch (Exception ex)
            {
                _logger.Error($"Исключение при удалении ролей для пользователя {userLogin}. Ex = {ex.Message}");

                return Result.CreateErrorResult(ex.Message);
            }
        }

        public ListResult<Permission> GetAllRoles()
        {
            _logger.Debug("Начало получения всех ролей");

            var query = $@"SELECT id,
                                  name,
                                  ""corporatePhoneNumber""
                           FROM ""{_schema}"".""ItRole""";

            try
            {
                var result = _connection.Query<RolePOCO>(query,
                    transaction: null,
                    commandTimeout: _timeOut);

                _logger.Debug("Успешное получение всех ролей");

                return ListResult<Permission>.CreateSuccessListResult(result.Convert());
            }
            catch (Exception ex)
            {
                _logger.Error($"Исключение при получении всех ролей. Ex = {ex.Message}");

                return ListResult<Permission>.CreateErrorListResult(message: ex.Message);
            }
        }

        public ListResult<string> GetUserRoles(string userLogin)
        {
            _logger.Debug($"Начало получения ролей для пользователя {userLogin}");

            var paramters = new DynamicParameters();
            paramters.Add("login", userLogin);

            var query = $@"SELECT ""roleId""
                           FROM ""{_schema}"".""UserITRole""
                           WHERE ""userId"" = @login";

            try
            {
                var result = _connection.Query<int>(query,
                    param: paramters,
                    transaction: null,
                    commandTimeout: _timeOut);

                _logger.Debug($"Успешное получение ролей для пользователя {userLogin}");

                return ListResult<string>.CreateSuccessListResult(result.ConvertRoleIds());
            }
            catch (Exception ex)
            {
                _logger.Error($"Исключение при получении ролей для пользователя {userLogin}. Ex = {ex.Message}");

                return ListResult<string>.CreateErrorListResult(message: ex.Message);
            }
        }

        #endregion
    }
}
