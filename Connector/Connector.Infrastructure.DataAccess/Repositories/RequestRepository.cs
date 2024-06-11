using Base.Models.Results;
using Connector.Core.Interfaces.DataAccess.Repositories;
using System.Data;
using Task.Integration.Data.Models.Models;
using Dapper;
using Connector.Infrastructure.DataAccess.Models.POCO;
using Connector.Infrastructure.DataAccess.Maps;
using ILogger = Task.Integration.Data.Models.ILogger;
using Connector.Infrastructure.DataAccess.Utils;

namespace Connector.Infrastructure.DataAccess.Repositories
{
    public class RequestRepository : IRequestRepository
    {
        #region Private

        private readonly IDbConnection _connection;
        private readonly IDbTransaction _transaction;
        private readonly string _schema;
        private readonly int _timeOut;
        private readonly ILogger _logger;

        #endregion

        public RequestRepository(IDbConnection connection, 
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

        public Result AddUserRequests(string userLogin, IEnumerable<int> requestIds)
        {
            _logger.Debug($"Начало добавления прав для пользователя {userLogin}");

            var filter = requestIds.FilterToDataAccess();

            var parameters = new DynamicParameters();
            parameters.Add("login", userLogin);
            parameters.Add(filter.name, filter.value);

            var requestQuery = @$"INSERT INTO ""{_schema}"".""UserRequestRight"" (""userId"", ""rightId"")
                                  SELECT @login,
                                         id
                                  FROM ""{_schema}"".""RequestRight""
                                  WHERE id = {filter.rowFilter}";

            try
            {
                _connection.Execute(requestQuery,
                    param: requestIds.Select(a => new { login = userLogin, ids = a }),
                    transaction: _transaction,
                    commandTimeout: _timeOut);

                _logger.Debug($"Успешное добавление прав для пользователя {userLogin}");

                return Result.CreateSuccessResult();
            }
            catch (Exception ex)
            {
                _logger.Error($"Исключение при добавлении прав для пользователя {userLogin}. Ex = {ex.Message}");

                return Result.CreateErrorResult(ex.Message);
            }
        }

        public Result DeleteUserRequests(string userLogin, IEnumerable<int> requestIds)
        {
            _logger.Debug($"Начало удалениея прав для пользователя {userLogin}");

            var filter = requestIds.FilterToDataAccess();

            var parameters = new DynamicParameters();
            parameters.Add("login", userLogin);
            parameters.Add(filter.name, filter.value);

            var requestQuery = $@"DELETE
                                  FROM ""{_schema}"".""UserRequestRight""
                                  WHERE ""userId"" = @login AND
                                        ""rightId"" = {filter.rowFilter}";

            try
            {
                _connection.Execute(requestQuery,
                    param: parameters,
                    transaction: _transaction,
                    commandTimeout: _timeOut);

                _logger.Debug($"Успешное удаление прав для пользователя {userLogin}");

                return Result.CreateSuccessResult();
            }
            catch (Exception ex)
            {
                _logger.Error($"Исключение при удалении прав для пользователя {userLogin}. Ex = {ex.Message}");

                return Result.CreateErrorResult(ex.Message);
            }
        }

        public ListResult<Permission> GetAllRequests()
        {
            _logger.Debug($"Начало получения всех прав");

            var query = $@"SELECT id,
                                  name
                           FROM ""{_schema}"".""RequestRight""";

            try
            {
                var result = _connection.Query<RequestPOCO>(query,
                    transaction: null,
                    commandTimeout: _timeOut);

                _logger.Debug("Успешное получение всех прав");

                return ListResult<Permission>.CreateSuccessListResult(result.Convert());
            }
            catch (Exception ex)
            {
                _logger.Error($"Исключение при получении всех прав. Ex = {ex.Message}");

                return ListResult<Permission>.CreateErrorListResult(message: ex.Message);
            }
        }

        public ListResult<string> GetUserRequests(string userLogin)
        {
            _logger.Debug($"Начало получения прав для пользователя {userLogin}");

            var paramters = new DynamicParameters();
            paramters.Add("login", userLogin);

            var query = $@"SELECT ""rightId""
                           FROM ""{_schema}"".""UserRequestRight""
                           WHERE ""userId"" = @login";

            try
            {
                var result = _connection.Query<int>(query,
                    param: paramters,
                    transaction: null,
                    commandTimeout: _timeOut);

                _logger.Debug($"Успешное получение прав для пользователя {userLogin}");

                return ListResult<string>.CreateSuccessListResult(result.ConvertRequestIds());
            }
            catch (Exception ex)
            {
                _logger.Error($"Исключение при получении прав для пользователя {userLogin}. Ex = {ex.Message}");

                return ListResult<string>.CreateErrorListResult(message: ex.Message);
            }
        }

        #endregion
    }
}
