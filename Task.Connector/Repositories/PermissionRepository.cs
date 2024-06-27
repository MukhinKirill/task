using Microsoft.EntityFrameworkCore;
using Task.Connector.Entities;
using Task.Connector.Parsers;
using Task.Connector.Parsers.Enums;
using Task.Integration.Data.Models.Models;

namespace Task.Connector.Repositories
{
    public class PermissionRepository : IPermissionRepository
    {
        public const string RequestRightGroupName = "Request";
        public const string ItRoleRightGroupName = "Role";
        public const string Delimeter = ":";

        private readonly TaskDbContext _dbContext;
        private readonly PermissionIdParser _permissionIdParser = new PermissionIdParser(RequestRightGroupName, ItRoleRightGroupName);

        public PermissionRepository(TaskDbContext dbContext) 
        {
            _dbContext = dbContext;
        }

        public void AddUserPermissions(string userLogin, IEnumerable<string> permissionIds)
        {
            var roleIds = new List<int>();
            var rightIds = new List<int>();

            foreach (var permission in permissionIds)
            {
                var permissionId = _permissionIdParser.Parse(permission);

                switch (permissionId.Type)
                {
                    case PermissionTypes.ItRole:
                        roleIds.Add(permissionId.Id);
                        break;
                    case PermissionTypes.RequestRight:
                        rightIds.Add(permissionId.Id);
                        break;
                }
            }

            AddUserRoles(userLogin, roleIds);
            AddUserRights(userLogin, rightIds);
        }

        public IEnumerable<Permission> GetAllPermissions()
        {
            var result = new List<Permission>();

            _dbContext.ItRoles.ToList().ForEach(itRole =>
            {
                var id = $"{ItRoleRightGroupName}{Delimeter}{itRole.Id}";
                result.Add(new Permission(id, itRole.Id.ToString(), itRole.Name));
            });

            _dbContext.RequestRights.ToList().ForEach(right =>
            {
                var id = $"{RequestRightGroupName}{Delimeter}{right.Id}";
                result.Add(new Permission(id, right.Id.ToString(), right.Name));
            });

            return result;
        }

        public IEnumerable<string> GetUserPermissions(string userLogin)
        {
            var result = new List<string>();
            var userRights = _dbContext.RequestRights.Join(_dbContext.UserRequestRights,
                right => right.Id,
                userRight => userRight.RightId,
                (right, userRight) => new { Login = userRight.UserId, RightDescription = right.Name })
                .Where(@object => @object.Login == userLogin)
                .ToList();
            var userRoles = _dbContext.UserItroles.Join(_dbContext.ItRoles,
                userRole => userRole.RoleId,
                itRole => itRole.Id,
                (userRole, itRole) => new { Login = userRole.UserId, ItRole = itRole.Name })
                .Where(@object => @object.Login == userLogin)
                .ToList();

            userRights.ForEach(right =>
            {
                result.Add($"{RequestRightGroupName}{Delimeter}{right.RightDescription}");
            });

            userRoles.ForEach(role =>
            {
                result.Add($"{ItRoleRightGroupName}{Delimeter}{role.ItRole}");
            });

            return result;
        }

        public void RemoveUserPermissions(string userLogin, IEnumerable<string> permissionIds)
        {
            var roleIds = new List<int>();
            var rightIds = new List<int>();

            foreach(var permission in permissionIds)
            {
                var permissionId = _permissionIdParser.Parse(permission);

                switch (permissionId.Type)
                {
                    case PermissionTypes.ItRole:
                        roleIds.Add(permissionId.Id);
                        break;
                    case PermissionTypes.RequestRight:
                        rightIds.Add(permissionId.Id);
                        break;
                }
            }

            RemoveUserRoles(userLogin, roleIds);
            RemoveUserRights(userLogin, rightIds);
        }

        private void AddUserRoles(string userLogin, IEnumerable<int> ids)
        {
            var userRoles = new List<UserItRole>();

            foreach (var id in ids)
            {
                var userItRole = new UserItRole();
                userItRole.UserId = userLogin;
                userItRole.RoleId = id;

                userRoles.Add(userItRole);
            }

            _dbContext.UserItroles.AddRange(userRoles);
            _dbContext.SaveChanges();
        }

        private void AddUserRights(string userLogin, IEnumerable<int> ids)
        {
            var userRights = new List<UserRequestRight>();

            foreach (var id in ids)
            {
                var userRight = new UserRequestRight();
                userRight.UserId = userLogin;
                userRight.RightId = id;

                userRights.Add(userRight);  
            }

            _dbContext.UserRequestRights.AddRange(userRights);
            _dbContext.SaveChanges();
        }

        private void RemoveUserRoles(string userLogin, IEnumerable<int> ids)
        {
            _dbContext.UserItroles
                .Where(user => user.UserId == userLogin)
                .Where(role => ids.Contains(role.RoleId))
                .ExecuteDelete();
        }

        private void RemoveUserRights(string userLogin, IEnumerable<int> ids)
        {
            _dbContext.UserRequestRights
                .Where(user => user.UserId == userLogin)
                .Where(right => ids.Contains(right.RightId))
                .ExecuteDelete();
        }
    }
}
