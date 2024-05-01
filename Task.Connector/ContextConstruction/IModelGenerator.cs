using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Task.Connector.ContextConstruction
{
    // Используется для заготовки объектных моделей и передачи их в PooledDynamicContextFactory
    // По сути содержит в себе настройку модели через Fluent API,
    // которая обычно содержится в методе OnModelCreating
    public interface IModelGenerator<T> where T : DbContext
    {
        IModel GenerateModel(string schemaName);
    }
}
