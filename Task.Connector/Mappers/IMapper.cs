namespace Task.Connector.Mappers
{
    public interface IMapper<TIn,TOut>
    {
        TOut Map(TIn @object);
    }
}
