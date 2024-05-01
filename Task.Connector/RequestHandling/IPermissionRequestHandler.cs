using Task.Connector.ContextConstruction.ContextFactory;
using Task.Connector.ContextConstruction.PermissionContext;
using Task.Integration.Data.Models.Models;

namespace Task.Connector.RequestHandling
{
    public interface IPermissionRequestHandler
    {
        public void Initialize(IDynamicContextFactory<DynamicPermissionContext>[] contextFactories, string schemaName);

        public IEnumerable<Permission> GetAllPermissions();

        public void AddUserPermissions(string userLogin, IEnumerable<string> rightIds);

        public void RemoveUserPermissions(string userLogin, IEnumerable<string> rightIds);

        public IEnumerable<string> GetUserPermissions(string userLogin);
    }
}
