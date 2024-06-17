using System.Data;
using Task.Connector.Database;
using Task.Integration.Data.DbCommon.DbModels;
using Task.Integration.Data.Models;

namespace Task.Connector.Services.UserPermission
{
    public class UserPermissionService : IUserPermission
    {
        private DataBaseContext _db;
        private readonly ILogger _logger;

        public UserPermissionService(DataBaseContext db, ILogger logger)
        {
            (_db, _logger) = (db, logger);
        }

        public void AddUserPermissions(string userLogin, IEnumerable<string> permissionIds)
            => ModifyUserPermissions(userLogin, permissionIds, AddUserPermissionByType);
        public void RemoveUserPermissions(string userLogin, IEnumerable<string> permissionIds)
            => ModifyUserPermissions(userLogin, permissionIds, RemoveUserPermissionByType);

        public IEnumerable<string> GetUserPermissions(string userLogin)
        {
            try
            {
                if (!_db.Users.Any(u => u.Login == userLogin))
                {
                    throw new DataException("User doesn't exist");
                }

                IEnumerable<string> roles = _db.UserITRoles
                    .Where(ur => ur.UserId == userLogin)
                    .Join(
                        _db.ITRoles,
                        ur => ur.RoleId,
                        r => r.Id,
                        (ur, r) => r.Name);

                IEnumerable<string> rights = _db.UserRequestRights
                    .Where(ur => ur.UserId == userLogin)
                    .Join(
                        _db.RequestRights,
                        ur => ur.RightId,
                        r => r.Id,
                        (ur, r) => r.Name);

                var userPermissions = roles.Concat(rights);
                _logger?.Debug("[UserPermission][Get] - success");
                return userPermissions;
            }
            catch (Exception ex)
            {
                _logger?.Error($"[UserPermission][Get] - error: {ex.Message}");
                return null;
            }
        }

        private void ModifyUserPermissions(string userLogin, IEnumerable<string> permissionIds, Action<PermissionDataModel, string> modifyPermission)
        {
            using (var transaction = _db.Database.BeginTransaction())
            {
                try
                {
                    if (!_db.Users.Any(u => u.Login == userLogin))
                    {
                        throw new DataException($"User with login = {userLogin} doesn't exist");
                    }

                    if (!permissionIds.All(PermissionHelper.IsPermissionDataValid))
                    {
                        throw new ArgumentException($"Permissions have invalid format");
                    }

                    permissionIds
                        .ToList()
                        .ForEach(pid => modifyPermission(PermissionHelper.SplitPermissionData(pid), userLogin));

                    _db.SaveChanges();
                    transaction.Commit();
                    _logger?.Debug($"[UserPermission][{modifyPermission.Method.Name}] - success");
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    _logger?.Error($"[UserPermission][{modifyPermission.Method.Name}] - error: {ex.Message}");
                }
            }
        }

        private void AddUserPermissionByType(PermissionDataModel permissionData, string userLogin)
        {
            switch (permissionData.Type)
            {
                case PermissionTypes.RequestRight:
                    int rightId = permissionData.Id;
                    if (!_db.RequestRights.Any(r => r.Id == rightId))
                    {
                        throw new DataException($"RequestRights with id = {rightId} doesn't exist");
                    }

                    if (_db.UserRequestRights.Any(ur => ur.UserId == userLogin && ur.RightId == rightId))
                    {
                        throw new DataException($"UserRequestRights with id = {rightId} for User with login = {userLogin} already exists");
                    }

                    var userRequestRight = new UserRequestRight()
                    {
                        RightId = rightId,
                        UserId = userLogin
                    };

                    _db.UserRequestRights.Add(userRequestRight);
                    break;
                case PermissionTypes.ITRole:
                    int roleId = permissionData.Id;
                    if (!_db.ITRoles.Any(r => r.Id == roleId))
                    {
                        throw new DataException($"ITRole with id = {roleId} doesn't exist");
                    }

                    if (_db.UserITRoles.Any(ur => ur.UserId == userLogin && ur.RoleId == roleId))
                    {
                        throw new DataException($"UserITRoles with id = {roleId} for User with login = {userLogin} already exists");
                    }

                    var userITRole = new UserITRole()
                    {
                        RoleId = roleId,
                        UserId = userLogin
                    };

                    _db.UserITRoles.Add(userITRole);
                    break;
            }
        }

        private void RemoveUserPermissionByType(PermissionDataModel permissionData, string userLogin)
        {
            switch (permissionData.Type)
            {
                case PermissionTypes.RequestRight:
                    int rightId = permissionData.Id;
                    if (!_db.RequestRights.Any(r => r.Id == rightId))
                    {
                        throw new DataException($"RequestRights with id = {rightId} doesn't exist");
                    }

                    var userRequestRight = _db.UserRequestRights.FirstOrDefault(ur => ur.UserId == userLogin && ur.RightId == rightId);
                    if (userRequestRight != null)
                    {
                        _db.UserRequestRights.Remove(userRequestRight);
                    }
                    break;
                case PermissionTypes.ITRole:
                    int roleId = permissionData.Id;
                    if (!_db.ITRoles.Any(r => r.Id == roleId))
                    {
                        throw new DataException($"ITRole with id = {roleId} doesn't exist");
                    }

                    var userITRole = _db.UserITRoles.FirstOrDefault(ur => ur.UserId == userLogin && ur.RoleId == roleId);
                    if (userITRole != null)
                    {
                        _db.UserITRoles.Remove(userITRole);
                    }
                    break;
            }
        }
    }
}