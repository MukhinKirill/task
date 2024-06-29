using Microsoft.EntityFrameworkCore;
using Task.Connector.Entities;
using Task.Connector.Parsers;
using Task.Connector.Parsers.Enums;
using Task.Connector.Parsers.Records;
using Task.Integration.Data.Models.Models;

namespace Task.Connector.Repositories
{
    public class PermissionRepository : IPermissionRepository
    {
        private const string RequestRightGroupName = "Request";
        private const string ItRoleRightGroupName = "Role";
        private const string Delimeter = ":";
       
        private readonly TaskDbContext _dbContext;
        private readonly PermissionIdParser _permissionIdParser;

        public PermissionRepository(TaskDbContext dbContext) 
        {
            _dbContext = dbContext;
            _permissionIdParser = new PermissionIdParser(new PermissionParserConfiguration(RequestRightGroupName, ItRoleRightGroupName, Delimeter));
        }

        public void AddUserPermissions(string userLogin, IEnumerable<string> permissionIds)
        {
            foreach (var permission in permissionIds)
            {
                var permissionId = _permissionIdParser.Parse(permission);

                switch (permissionId.Type)
                {
                    case PermissionTypes.ItRole:
                        _dbContext.UserItroles.Add(new UserItRole()
                        {
                            UserId = userLogin,
                            RoleId = permissionId.Id,
                        });

                        break;

                    case PermissionTypes.RequestRight:
                        _dbContext.UserRequestRights.Add(new UserRequestRight()
                        {
                            UserId = userLogin,
                            RightId = permissionId.Id,
                        });

                        break;
                }
            }
            
            if(permissionIds.Count() > 0)
            {
                _dbContext.SaveChanges();
            }
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
                (right, userRight) => new { Login = userRight.UserId, RightId = right.Id, RightDescription = right.Name })
                .Where(@object => @object.Login == userLogin)
                .ToList();

            var userRoles = _dbContext.UserItroles.Join(_dbContext.ItRoles,
                userRole => userRole.RoleId,
                itRole => itRole.Id,
                (userRole, itRole) => new { Login = userRole.UserId, ItRoleId = itRole.Id, ItRoleName = itRole.Name })
                .Where(@object => @object.Login == userLogin)
                .ToList();

            userRights.ForEach(right =>
            {
                result.Add($"{RequestRightGroupName}{Delimeter}{right.RightId} - {right.RightDescription}");
            });

            userRoles.ForEach(role =>
            {
                result.Add($"{RequestRightGroupName}{Delimeter}{role.ItRoleId} - {role.ItRoleName}");
            });

            return result;
        }

        public void RemoveUserPermissions(string userLogin, IEnumerable<string> permissionIds)
        {
            var dictPermissions = new Dictionary<PermissionTypes, List<int>>
            {
                { PermissionTypes.ItRole, new List<int>() },
                { PermissionTypes.RequestRight, new List<int>() }
            };

            foreach (var permission in permissionIds)
            {
                var permissionId = _permissionIdParser.Parse(permission);
                dictPermissions[permissionId.Type].Add(permissionId.Id);
            }

            _dbContext.UserItroles.Where(user => user.UserId == userLogin).Where(role => dictPermissions[PermissionTypes.ItRole].Contains(role.RoleId)).ExecuteDelete();
            _dbContext.UserRequestRights.Where(user => user.UserId == userLogin).Where(right => dictPermissions[PermissionTypes.RequestRight].Contains(right.RightId)).ExecuteDelete();
        }
    }
}
