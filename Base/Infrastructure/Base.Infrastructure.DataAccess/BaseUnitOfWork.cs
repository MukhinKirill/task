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
            ConnectToDb(connectionString);
        }

        public virtual void BeginTran()
        {
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

        protected abstract void ConnectToDb(string connectionString);

        #endregion
    }
}
