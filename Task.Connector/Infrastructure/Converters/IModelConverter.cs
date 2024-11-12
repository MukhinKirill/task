namespace Task.Connector.Infrastructure.Converters;

public interface IModelConverter<Tin, TOut>
where Tin : class
where TOut: class
{
    TOut Convert(Tin tIn);
}