using Connector.Core.Interfaces.DataAccess;
using Connector.Infrastructure.DataAccess;
using Connector.Utils.Logger;
using Core.Models.Options;
using System.Data;
using Task.Integration.Data.Models;
using Task.Integration.Data.Models.Models;

namespace Task.Connector
{
    public class ConnectorDb : IConnector
    {
        #region Private

        private const string requestRightGroupName = "Request";
        private const string itRoleRightGroupName = "Role";
        private const string delimeter = ":";

        private IUnitOfWork _unitOfWork;

        #endregion

        #region Properties

        public ILogger Logger { get; set; }

        #endregion

        #region Methods

        public void StartUp(string connectionString)
        {
            var dbOptions = GetDbOptions(connectionString);

            Logger = new FileLogger($"{DateTime.Now}connectorPOSTGRE.Log", $"{DateTime.Now}connectorPOSTGRE");
            _unitOfWork = new UnitOfWork(dbOptions, Logger);
        }

        public void CreateUser(UserToCreate user)
        {
            _unitOfWork.BeginTran();

            var result = _unitOfWork.UserRepository.CreateUser(user);

            if (result.IsError)
            {
                _unitOfWork.RollBack();

                return;
            }

            _unitOfWork.Commit();
        }

        public IEnumerable<Property> GetAllProperties()
        {
            var result = _unitOfWork.UserRepository.GetAllProperties();

            if (result.IsError)
            {
                return Enumerable.Empty<Property>();
            }

            return result.Value;
        }

        public IEnumerable<UserProperty> GetUserProperties(string userLogin)
        {
            var result = _unitOfWork.UserRepository.GetUserProperties(userLogin);

            if (result.IsError)
            {
                return Enumerable.Empty<UserProperty>();
            }

            return result.Value;
        }

        public bool IsUserExists(string userLogin)
        {
            var result = _unitOfWork.UserRepository.IsUserExists(userLogin);

            return result.Value;
        }

        public void UpdateUserProperties(IEnumerable<UserProperty> properties, string userLogin)
        {
            _unitOfWork.BeginTran();

            var result = _unitOfWork.UserRepository.UpdateUser(userLogin, properties);

            if (result.IsError )
            {
                _unitOfWork.RollBack();

                return;
            }

            _unitOfWork.Commit();
        }

        public IEnumerable<Permission> GetAllPermissions()
        {
            var result = new List<Permission>();

            var requestResult = _unitOfWork.RequestRepository.GetAllRequests();

            if (requestResult.IsError)
            {
                return result;
            }

            result.AddRange(requestResult.Value);

            var roleResult = _unitOfWork.RoleRepository.GetAllRoles();

            if (requestResult.IsError)
            {
                return Enumerable.Empty<Permission>();
            }

            result.AddRange(roleResult.Value);

            return result;
        }

        public void AddUserPermissions(string userLogin, IEnumerable<string> rightIds)
        {
            if (!rightIds.Any())
            {
                Logger.Warn("Попытка добавить права с пустым списком rightIds");

                return;
            }

            var ids = GetIds(rightIds);
            if (!ids.requestIds.Any() && !ids.roleIds.Any())
            {
                Logger.Warn("Не удалось найти подходящих RoleId и RequestId");

                return;
            }

            _unitOfWork.BeginTran();

            if (ids.requestIds.Any())
            {
                var requestResult = _unitOfWork.RequestRepository.AddUserRequests(userLogin, ids.requestIds);

                if (requestResult.IsError)
                {
                    _unitOfWork.RollBack();

                    return;
                }
            }

            if (ids.roleIds.Any())
            {
                var roleResult = _unitOfWork.RoleRepository.AddUserRoles(userLogin, ids.roleIds);

                if (roleResult.IsError)
                {
                    _unitOfWork.RollBack();

                    return;
                }
            }

            _unitOfWork.Commit();
        }

        public void RemoveUserPermissions(string userLogin, IEnumerable<string> rightIds)
        {
            if (!rightIds.Any())
            {
                Logger.Warn("Попытка удалить права с пустым списком rightIds");

                return;
            }

            var ids = GetIds(rightIds);
            if (! ids.requestIds.Any() && ! ids.roleIds.Any())
            {
                Logger.Warn("Не удалось найти подходящих RoleId и RequestId");

                return;
            }

            _unitOfWork.BeginTran();

            if (ids.requestIds.Any())
            {
                var requestResult = _unitOfWork.RequestRepository.DeleteUserRequests(userLogin, ids.requestIds);

                if (requestResult.IsError)
                {
                    _unitOfWork.RollBack();

                    return;
                }
            }

            if (ids.roleIds.Any())
            {
                var roleResult = _unitOfWork.RoleRepository.DeleteUserRoles(userLogin, ids.roleIds);

                if (roleResult.IsError)
                {
                    _unitOfWork.RollBack();

                    return;
                }
            }

            _unitOfWork.Commit();
        }

        public IEnumerable<string> GetUserPermissions(string userLogin)
        {
            var result = new List<string>();

            var requestResult = _unitOfWork.RequestRepository.GetUserRequests(userLogin);

            if (requestResult.IsError)
            {
                return result;
            }

            result.AddRange(requestResult.Value);

            var roleResult = _unitOfWork.RoleRepository.GetUserRoles(userLogin);

            if (roleResult.IsError)
            {
                return Enumerable.Empty<string>();
            }

            result.AddRange(roleResult.Value);

            return result;
        }

        #endregion

        #region Private Methods

        private (List<int> roleIds, List<int> requestIds) GetIds(IEnumerable<string> rightIds)
        {
            var requestsIds = new List<int>();
            var rolesIds = new List<int>();
            foreach (var right in rightIds.Select(a => a.Split(delimeter)).ToList())
            {
                if (right[0] == itRoleRightGroupName)
                {
                    rolesIds.Add(int.Parse(right[1]));
                    continue;
                }
                if (right[0] == requestRightGroupName)
                {
                    requestsIds.Add(int.Parse(right[1]));
                    continue;
                }
            }

            return (rolesIds, requestsIds);
        }

        private DbOptions GetDbOptions(string connection)
        {
            var parameters = new Dictionary<string, string>();

            int i = 0;
            while (i < connection.Length)
            {
                string key = "";
                string value = "";

                while (connection[i] != '=')
                {
                    key += connection[i++];
                }

                i += 2;

                while (connection[i] != '\'')
                {
                    value += connection[i++];
                }

                i += 2;

                parameters.Add(key, value);
            }

            return new DbOptions()
            {
                ConnectionString = parameters[nameof(DbOptions.ConnectionString)],
                Schema = parameters[nameof(DbOptions.Schema)],
                CommandTimeOut = int.Parse(parameters[nameof(DbOptions.CommandTimeOut)])
            };
        }

        #endregion
    }
}