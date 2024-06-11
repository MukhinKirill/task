using Base.Infrastructure.DataAccess;
using Connector.Core.Interfaces.DataAccess;
using Connector.Core.Interfaces.DataAccess.Repositories;
using Connector.Infrastructure.DataAccess.Repositories;
using Core.Models.Options;
using Npgsql;
using System.Data;
using Task.Integration.Data.Models;

namespace Connector.Infrastructure.DataAccess
{
    public class UnitOfWork : BaseUnitOfWork, IUnitOfWork
    {
        #region Private

        private readonly DbOptions _options;
        private readonly ILogger _logger;

        #endregion

        public UnitOfWork(DbOptions options, ILogger logger) : base(options.ConnectionString) 
        { 
            _options = options;
            _logger = logger;
        }

        #region Protected Methods

        protected override void ConnectToDb(string connectionString)
        {
            try
            {
                _connection =  new NpgsqlConnection(connectionString);
                _connection.Open();
            }
            catch (Exception ex)
            {
                _logger.Error($"Исключение при подключении к базе данных. Ex = {ex.Message}");
                throw;
            }
        }

        #endregion

        #region Repositories

        public IRequestRepository RequestRepository => 
            new RequestRepository(_connection, _transaction, _options.Schema, _options.CommandTimeOut, _logger);

        public IRoleRepository RoleRepository =>
            new RoleRepository(_connection, _transaction, _options.Schema, _options.CommandTimeOut, _logger);

        public IUserRepository UserRepository =>
            new UserRepository(_connection, _transaction, _options.Schema, _options.CommandTimeOut, _logger);

        #endregion
    }
}
