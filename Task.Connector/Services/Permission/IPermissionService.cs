namespace Task.Connector.Services.Permission
{
    public interface IPermissionService
    {
        public IEnumerable<Integration.Data.Models.Models.Permission> GetAllPermissions();
    }
}