using Task.Connector.ContextConstruction.ContextFactory;
using Task.Connector.ContextConstruction.UserContext;

using UserObj = System.Collections.Generic.Dictionary<string, object>;
using Properties = System.Collections.Generic.Dictionary<string, string>;

namespace Task.Connector.RequestHandling
{
    public interface IRawUserRequestHandler
    {
        // Не уверен, стоило ли добавить параметр string schemaName -
        // текущая реализация RawUserRequestHandler им не пользуется
        public void Initialize(IDynamicContextFactory<DynamicUserContext> contextFactory);

        public void CreateUser(UserObj user);

        public Properties GetAllProperties();

        public UserObj GetUserProperties(string userLogin);

        public bool IsUserExists(string userLogin);

        public void UpdateUserProperties(UserObj user);
    }
}
