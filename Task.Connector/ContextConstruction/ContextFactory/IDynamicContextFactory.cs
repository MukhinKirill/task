using Microsoft.EntityFrameworkCore;

namespace Task.Connector.ContextConstruction.ContextFactory
{
    public interface IDynamicContextFactory<T> where T : DbContext
    {
        T CreateContext();
    }
}
