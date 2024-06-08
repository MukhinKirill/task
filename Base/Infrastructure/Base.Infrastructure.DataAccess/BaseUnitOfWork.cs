using System.Data;

namespace Base.Infrastructure.DataAccess
{
    public abstract class BaseUnitOfWork
    {
        private bool _disposed;

        protected IDbConnection _connection;
        protected IDbTransaction _transaction;

        public BaseUnitOfWork(string connectionString)
        {
            _connection = DbConnect(connectionString);
        }

        public virtual void BeginTran()
        {
            if (_connection.State is not ConnectionState.Open)
                _connection.Open();

            _transaction = _connection.BeginTransaction();
        }

        public virtual void Commit()
        {
            _transaction.Commit();
        }

        public virtual void RollBack()
        {
            _transaction.Rollback();
        }

        #region Dispose 

        public virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _connection.Close();
                    _connection.Dispose();
                }
                _disposed = true;
            }
        }

        public virtual void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion

        #region Protected

        protected abstract IDbConnection DbConnect(string connectionString);

        #endregion
    }
}
