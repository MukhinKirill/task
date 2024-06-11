using Base.Models.Results;
using Connector.Core.Interfaces.DataAccess.Repositories;
using Connector.Infrastructure.DataAccess.Models.POCO;
using Dapper;
using System.Data;
using Task.Integration.Data.Models.Models;
using ILogger = Task.Integration.Data.Models.ILogger;

namespace Connector.Infrastructure.DataAccess.Repositories
{
    public class UserRepository : IUserRepository
    {
        #region Private

        private readonly IDbConnection _connection;
        private readonly IDbTransaction _transaction;
        private readonly string _schema;
        private readonly int _timeOut;
        private readonly ILogger _logger;

        #endregion

        public UserRepository(IDbConnection connction,
            IDbTransaction transaction,
            string schema,
            int timeOut,
            ILogger logger) 
        { 
            _connection = connction;
            _transaction = transaction;
            _schema = schema;
            _timeOut = timeOut;
            _logger = logger;
        }

        #region Methods

        public Result CreateUser(UserToCreate user)
        {
            _logger.Debug($"Начало создания нового пользователя. Login = {user.Login}");

            var userPoco = new UserPOCO(user.Login, user.Properties);

            var paramters = new DynamicParameters();
            paramters.Add("userId", userPoco.Login);
            paramters.Add("password", user.HashPassword);
            paramters.Add("lastName", userPoco.LastName);
            paramters.Add("firstName", userPoco.FirstName);
            paramters.Add("middleName", userPoco.MiddleName);
            paramters.Add("telephoneNumber", userPoco.TelephoneNumber);
            paramters.Add("isLead", userPoco.IsLead);

            var query = @$"INSERT INTO ""{_schema}"".""Passwords"" (""userId"", ""password"")
                           VALUES (@userId,
                                   @password);

                           INSERT INTO ""{_schema}"".""User"" (login, ""firstName"", ""lastName"", ""middleName"", ""telephoneNumber"", ""isLead"")
                           VALUES (@userId,
                                   @firstName,
                                   @lastName,
                                   @middleName,
                                   @telephoneNumber,
                                   @isLead);";

            try
            {
                _connection.Execute(query,
                    param: paramters,
                    transaction: _transaction,
                    commandTimeout: _timeOut);

                _logger.Debug($"Успешное создание нового пользователя. Login = {user.Login}");

                return Result.CreateSuccessResult();
            }
            catch (Exception ex)
            {
                _logger.Error($"Ошибка при создании нового пользователя. Login = {user.Login}. Ex = {ex.Message}");

                return Result.CreateErrorResult(ex.Message);
            }
        }

        public ListResult<Property> GetAllProperties()
        {
            _logger.Debug($"Начало получения всех полей");

            string[] tables = new[] { "User", "Passwords" };
            string[] columns = new[] { "login", "id", "userId" };

            var parameters = new DynamicParameters();
            parameters.Add("tables", tables);
            parameters.Add("columns", columns);

            var query = @"SELECT c.column_name AS Name,
                                 pgd.description AS Description
                          FROM information_schema.columns c LEFT JOIN
	                           pg_catalog.pg_description pgd ON pgd.objsubid = c.ordinal_position
                          WHERE c.table_name = ANY(@tables) AND
	                            c.column_name <> ALL(@columns);";

            try
            {
                var result = _connection.Query<Property>(query,
                    param: parameters,
                    transaction: null,
                    commandTimeout: _timeOut);

                _logger.Debug("Успешное получение всех полей");

                return ListResult<Property>.CreateSuccessListResult(result);
            }
            catch (Exception ex)
            {
                _logger.Error($"Ошибка при получении всех полей. Ex = {ex.Message}");

                return ListResult<Property>.CreateErrorListResult(message: ex.Message);
            }
        }

        public ListResult<UserProperty> GetUserProperties(string userLogin)
        {
            _logger.Debug($"Получение полей для пользователя {userLogin}");

            var pocoUsers = GetUsers(userLogin);
            if (pocoUsers.IsError)
            {
                return ListResult<UserProperty>.CreateErrorListResult(message: pocoUsers.Message);
            }

            _logger.Debug($"Успешное получение полей для пользователя {userLogin}");

            return ListResult<UserProperty>.CreateSuccessListResult(pocoUsers.Value.First().GetProperty());
        }

        public Result<bool> IsUserExists(string userLogin)
        {
            _logger.Debug($"Проверка на наличие пользователя {userLogin}");

            var pocoUsers = GetUsers(userLogin);
            if (pocoUsers.IsError)
            {
                return Result<bool>.CreateErrorResult(false, pocoUsers.Message);
            }

            var result = !(pocoUsers is null || pocoUsers.Value.Count() == 0);

            _logger.Debug($"Пользователь {userLogin} {(result ? "" : "не")} найден");

            return Result<bool>.CreateSuccessResult(result);
        }

        public Result UpdateUser(string userLogin, IEnumerable<UserProperty> properties)
        {
            _logger.Debug($"Начало обновления полей для пользователя {userLogin}");

            if (properties.Count() == 0)
            {
                _logger.Debug($"Нет полей для обновления для пользователя {userLogin}");

                return Result.CreateSuccessResult();
            }

            var parameters = new DynamicParameters();
            parameters.Add("login", userLogin);

            foreach (var prop in properties)
            {
                if (prop.Name == "isLead")
                {
                    parameters.Add(prop.Name, prop.Value == "true");
                    continue;
                }

                parameters.Add(prop.Name, prop.Value);
            }

            var query = $@"UPDATE ""{_schema}"".""User""
                           SET {string.Join(Environment.NewLine, properties.Select(a => "\"" + a.Name + "\"" + " = @" + a.Name))}
                           WHERE login = @login";

            try
            {
                _connection.Execute(query,
                    param: parameters,
                    transaction: _transaction,
                    commandTimeout: _timeOut);

                _logger.Debug($"Успешное изменение полей для пользователя {userLogin}");

                return Result.CreateSuccessResult();
            }
            catch (Exception ex)
            {
                _logger.Error($"Ошибка при изменении полей для пользователя {userLogin}. Ex = {ex.Message}");

                return Result.CreateErrorResult(ex.Message);
            }
        }

        #endregion

        #region Private Methods

        private ListResult<UserPOCO> GetUsers(string userLogin)
        {
            _logger.Debug($"Начало получения пользователя {userLogin}");

            var parameters = new DynamicParameters();
            parameters.Add("login", userLogin);

            var query = $@"SELECT *
                           FROM ""{_schema}"".""User""
                           WHERE login = @login";

            try
            {
                var result = _connection.Query<UserPOCO>(query,
                    param: parameters,
                    transaction: null,
                    commandTimeout: _timeOut);

                _logger.Debug($"Успешное получение пользователя {userLogin}");

                return ListResult<UserPOCO>.CreateSuccessListResult(result);
            }
            catch (Exception ex)
            {
                _logger.Error($"Ошибка при получении пользователя {userLogin}. Ex = {ex.Message}");

                return ListResult<UserPOCO>.CreateErrorListResult(message: ex.Message);
            }
        }

        #endregion
    }
}
