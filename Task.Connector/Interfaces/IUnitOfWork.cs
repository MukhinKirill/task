namespace Task.Connector.Interfaces
{
    public interface IUnitOfWork : IDisposable
    {
        int Commit();
    }
}
