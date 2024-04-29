namespace Task.Connector.ContextConstruction.Converter
{
    public interface IConverter
    {
        // Добавляет конверcию свойств сущностей в другие типы
        // для работы с полями в БД
        void AddConversion(string type, Microsoft.EntityFrameworkCore.Metadata.Builders.PropertyBuilder builder);
    }
}
