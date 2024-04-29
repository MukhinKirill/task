namespace Task.Connector.ContextConstruction.Converter
{
    public class DefaultConverter : IConverter
    {

        // Добавляет конверцию свойства сущности в boolean или integer
        public void AddConversion(string type, Microsoft.EntityFrameworkCore.Metadata.Builders.PropertyBuilder builder)
        {
            switch (type)
            {
                case "boolean":
                    builder.HasConversion<bool>();
                    break;
                case "integer":
                    builder.HasConversion<int>();
                    break;
            }
        }
    }
}
