namespace Task.Connector.Models.ModelConversion
{
    public interface IPermissionConverter
    {
        IEnumerable<UserPermission> ConstructUserPermissions(string userLogin, IEnumerable<string> rightIds);

        IEnumerable<string> ExtractIdFromUserPermissions(IEnumerable<UserPermission> permissions);
    }
}
