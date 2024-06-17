using Task.Connector.Database;
using Task.Integration.Data.Models;
using PermissionEntity = Task.Integration.Data.Models.Models.Permission;

namespace Task.Connector.Services.Permission
{
    public class PermissionService : IPermissionService
    {
        private DataBaseContext _db;
        private readonly ILogger _logger;

        public PermissionService(DataBaseContext db, ILogger logger)
        {
            (_db, _logger) = (db, logger);
        }

        public IEnumerable<PermissionEntity> GetAllPermissions()
        {
            try
            {
                IEnumerable<PermissionEntity> roles = _db.ITRoles.Select(r => new PermissionEntity(r.Id.ToString(), r.Name, string.Empty));
                IEnumerable<PermissionEntity> rights = _db.RequestRights.Select(r => new PermissionEntity(r.Id.ToString(), r.Name, string.Empty));
                var permissions = roles.Concat(rights);
                _logger?.Debug("[Permission][GetAll] - success");
                return permissions;
            }
            catch (Exception ex)
            {
                _logger?.Error($"[Permission][GetAll] - error: {ex.Message}");
                return null;
            }
        }
    }
}