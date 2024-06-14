using System.Linq;
using Task.Connector.Repositories;
using Task.Connector.Repositories.Interfaces;
using Task.Connector.Services.Interfaces;
using Task.Integration.Data.DbCommon.DbModels;
using Task.Integration.Data.Models.Models;

namespace Task.Connector.Services
{
    public class PermissionService : IPermissionService
    {
        private const string RightTypeRequest = "Request";
        private const string RightTypeRole = "Role";
        private const string PermissionTypeRights = "Права";
        private const string PermissionTypeRoles = "Роли";
        
        private readonly IRequestRightRepository _requestRightRepository;
        private readonly IITRoleRepository _itRoleRepository;
        private readonly IUserITRoleRepository _userItRoleRepository;
        private readonly IUserRequestRightRepository _userRequestRightRepository;
        
        public PermissionService(string connectionString)
        {
            _requestRightRepository = new RequestRightRepository(connectionString);
            _itRoleRepository = new ITRoleRepository(connectionString);
            _userItRoleRepository = new UserITRoleRepository(connectionString);
            _userRequestRightRepository = new UserRequestRightRepository(connectionString);
        }

        public IEnumerable<Permission> GetAllPermissions()
        {
            var requestRights = _requestRightRepository.GetAllRequestRights()
                .Select(rr => new Permission(rr.Id.ToString(), rr.Name, PermissionTypeRights));

            var itRoles = _itRoleRepository.GetAllITRoles()
                .Select(ir => new Permission(ir.Id.ToString(), ir.Name, PermissionTypeRoles));

            return requestRights.Concat(itRoles);
        }

        public void AddUserPermissions(string userLogin, IEnumerable<string> rightIds)
        {
            var userItRoles = new List<UserITRole>();
            var userRequestRights = new List<UserRequestRight>();

            foreach (var rightId in rightIds)
            {
                (string type, int id) = ParseRightId(rightId);

                switch (type)
                {
                    case RightTypeRequest:
                        userRequestRights.Add(new UserRequestRight { UserId = userLogin, RightId = id });
                        break;
                    case RightTypeRole:
                        userItRoles.Add(new UserITRole { UserId = userLogin, RoleId = id });
                        break;
                }
            }

            _userItRoleRepository.AddUserITRole(userItRoles);
            _userRequestRightRepository.AddUserRequestRight(userRequestRights);
        }

        public void RemoveUserPermissions(string userLogin, IEnumerable<string> rightIds)
        {
            var userItRoles = _userItRoleRepository.GetAlluserITRole().ToList();
            var userRequestRights = _userRequestRightRepository.GetAllUserRequestRight().ToList();

            var userItRolesForDelete = new List<UserITRole>();
            var userRequestRightsForDelete = new List<UserRequestRight>();

            foreach (var rightId in rightIds)
            {
                (string type, int id) = ParseRightId(rightId);

                switch (type)
                {
                    case RightTypeRequest:
                        var userRequestRight = userRequestRights.FirstOrDefault(urr => urr.UserId == userLogin && urr.RightId == id);
                        if (userRequestRight != null)
                        {
                            userRequestRightsForDelete.Add(userRequestRight);
                        }
                        break;
                    case RightTypeRole:
                        var userItRole = userItRoles.FirstOrDefault(uir => uir.UserId == userLogin && uir.RoleId == id);
                        if (userItRole != null)
                        {
                            userItRolesForDelete.Add(userItRole);
                        }
                        break;
                }
            }

            _userItRoleRepository.RemoveUserITRole(userItRolesForDelete);
            _userRequestRightRepository.RemoveUserRequestRights(userRequestRightsForDelete);
        }

        public IEnumerable<string> GetUserPermissions(string userLogin)
        {
            var requestRights = _userRequestRightRepository
                .GetUserRequestsRightsByLogin(userLogin)
                .Select(urr => $"{RightTypeRequest}:{urr.RightId}")
                .ToList();

            var itRoles = _userItRoleRepository
                .GetUserITRolesByLogin(userLogin)
                .Select(uir => $"{RightTypeRole}:{uir.RoleId}")
                .ToList();

        return requestRights.Concat(itRoles);
        }

        private (string type, int id) ParseRightId(string rightId)
        {
            var splitIds = rightId.Split(':', 2);
            return (splitIds[0], int.Parse(splitIds[1]));
        }
    }
}
