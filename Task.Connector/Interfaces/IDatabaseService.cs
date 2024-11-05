using System.Data;

namespace Task.Connector.Interfaces;

public interface IDatabaseService
{
    IDbConnection GetOpenConnection();
    void ExecuteInTransaction(Action<IDbConnection, IDbTransaction> action);
    T ExecuteInTransaction<T>(Func<IDbConnection, IDbTransaction, T> func);
}

