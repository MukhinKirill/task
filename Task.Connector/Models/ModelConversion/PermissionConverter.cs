
namespace Task.Connector.Models.ModelConversion
{
    public class PermissionConverter : IPermissionConverter
    {
        public IEnumerable<UserPermission> ConstructUserPermissions(string userLogin, IEnumerable<string> rightIds)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<string> ExtractIdFromUserPermissions(IEnumerable<UserPermission> permissions)
        {
            throw new NotImplementedException();
        }
    }
}
