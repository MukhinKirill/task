namespace Task.Connector.Infrastructure.Converters;

public interface IModelConverter<Tin, TOut>
{
    TOut Convert(Tin modelIn);
}