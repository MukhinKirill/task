using Dapper;
using System.Data;
using Task.Connector.Models;
using Task.Connector.DbSchemes;
using Task.Integration.Data.Models;
using Task.Integration.Data.Models.Models;

namespace Task.Connector.Commands
{
    internal class CreateUserCommand
    {
        private static readonly string insertUserQuery = $@"INSERT INTO {UserScheme.TableName} 
                ({UserScheme.login}, {UserScheme.lastName}, {UserScheme.firstName}, {UserScheme.middleName}, {UserScheme.telephoneNumber}, {UserScheme.isLead})
                VALUES 
                (@{nameof(DbUser.Login)}, @{nameof(DbUser.LastName)}, @{nameof(DbUser.FirstName)}, @{nameof(DbUser.MiddleName)}, @{nameof(DbUser.TelephoneNumber)}, @{nameof(DbUser.IsLead)})";

        private static readonly string insertPasswordQuery = $@"INSERT INTO {PasswordsScheme.TableName} 
                ({PasswordsScheme.userId}, {PasswordsScheme.password} ) 
                VALUES 
                (@userId, @password)";

        private readonly IDbConnection _dbConnection;
        private readonly ILogger _logger;

        public CreateUserCommand(IDbConnection dbConnection, ILogger logger)
        {
            _dbConnection = dbConnection;
            _logger = logger;
        }

        public void Execute(UserToCreate user)
        {
            var isUserExists = new IsUserExistsQuery(_dbConnection, _logger);
            if (isUserExists.Execute(user.Login))
            {
                _logger.Error($"Trying to create user that already exist: {user.Login}");
                return;
            }

            using var transaction = _dbConnection.BeginTransaction();
            _dbConnection.Execute(insertUserQuery, new DbUser(user), transaction);
            _dbConnection.Execute(insertPasswordQuery, new { userId = user.Login, password = user.HashPassword }, transaction);
            transaction.Commit();
        }
    }
}
