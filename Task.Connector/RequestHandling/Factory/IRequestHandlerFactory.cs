namespace Task.Connector.RequestHandling.Factory
{
    public interface IRequestHandlerFactory<T>
    {
        public T CreateRequestHandler();
    }
}
