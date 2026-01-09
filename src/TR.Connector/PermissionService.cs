using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using TR.Connectors.Api.Entities.DTO.Responses.RoleResponse;
using TR.Connectors.Api.Entities.DTO.Responses.UserRoleResponse;
using TR.Connectors.Api.Entities.DTO.Responses.UserResponse;
using TR.Connectors.Api.Interfaces;
using TR.Connectors.Api.Entities.Model;
using TR.Connectors.Api.Enums;

namespace TR.Connector
{
    public class PermissionService: IPermissionService
    {
        private readonly IApiClient _apiClient;
        private readonly IAccessTypeParser _accessTypeParser;
        private readonly ILogger<PermissionService> _logger;

        public PermissionService(IApiClient apiClient, IAccessTypeParser accessTypeParser, ILogger<PermissionService> logger)
        {
            _apiClient = apiClient ?? throw new ArgumentNullException(nameof(apiClient));
            _accessTypeParser = accessTypeParser ?? throw new ArgumentNullException(nameof(accessTypeParser));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        private void IsUserLocked(UserResponseData user, string userLogin)
        {
            if (user.status == UserStatusType.Lock.ToString())
                throw new ArgumentException("Пользователь {userLogin} залочен", userLogin);
        }

        private async Task AddOrRemoveUserAccessAsync
            (
                IApiClient apiClient,
                IEnumerable<string> rightIds,
                string userLogin,
                string httpRequestType,
                CancellationToken ct
            )
        {
            foreach (var raw in rightIds)
            {
                if (!_accessTypeParser.TryParse(raw, out var type, out var id))
                    throw new InvalidCastException($"Тип доступа не определен: {raw}. {nameof(_accessTypeParser)}");

                if (!int.TryParse(id, out var intId))
                    throw new InvalidCastException($"Передан некорректный формат ID права/роли: {id}");
                switch (type)
                {
                    case AccessType.ItRole:
                        if(httpRequestType == HttpMethod.Put.ToString())
                            await apiClient.PutAsync(ApiEndpoints.AddRole(userLogin, intId), null!, ct).ConfigureAwait(false);
                        else if(httpRequestType == HttpMethod.Delete.ToString()) await _apiClient.DeleteAsync(ApiEndpoints.DropRole(userLogin, intId), ct).ConfigureAwait(false);
                        break;
                    case AccessType.RequestRight:
                        if (httpRequestType == HttpMethod.Put.ToString())
                            await apiClient.PutAsync(ApiEndpoints.AddRight(userLogin, intId), null!, ct).ConfigureAwait(false);
                        else if (httpRequestType == HttpMethod.Delete.ToString()) await _apiClient.DeleteAsync(ApiEndpoints.DropRight(userLogin, intId), ct).ConfigureAwait(false);
                        break;
                    default:
                        throw new NotSupportedException($"Неизвестный тип доступа: {type}");
                }
            }
        }

        public async Task<IEnumerable<Permission>> GetAllPermissionsAsync(CancellationToken ct = default)
        {
            var roles = await _apiClient.GetJsonAsync<RoleResponse>(ApiEndpoints.RolesAll, ct).ConfigureAwait(false);
            var rights = await _apiClient.GetJsonAsync<RoleResponse>(ApiEndpoints.RightsAll, ct).ConfigureAwait(false);

            var rolePerms = roles?.data?.Select(r => new Permission(_accessTypeParser.Format(AccessType.ItRole, r.id.ToString()), r.name!, r.corporatePhoneNumber!)) ?? Enumerable.Empty<Permission>();
            var rightPerms = rights?.data?.Select(r => new Permission(_accessTypeParser.Format(AccessType.RequestRight, r.id.ToString()), r.name!, r.corporatePhoneNumber!)) ?? Enumerable.Empty<Permission>();

            return rolePerms.Concat(rightPerms);
        }

        public async Task<IEnumerable<string>> GetUserPermissionsAsync(string userLogin, CancellationToken ct = default)
        {
            var userRolesResp = await _apiClient.GetJsonAsync<UserRoleResponse>(ApiEndpoints.UserRoles(userLogin), ct).ConfigureAwait(false);
            var userRightsResp = await _apiClient.GetJsonAsync<UserRoleResponse>(ApiEndpoints.UserRights(userLogin), ct).ConfigureAwait(false);

            var roleFormat = userRolesResp?.data?.Select(r => _accessTypeParser.Format(AccessType.ItRole, r.id.ToString())) ?? Enumerable.Empty<string>();
            var rightFormat = userRightsResp?.data?.Select(r => _accessTypeParser.Format(AccessType.RequestRight,r.id.ToString())) ?? Enumerable.Empty<string>();

            return roleFormat.Concat(rightFormat).ToList();
        }

        public async Task AddUserPermissionsAsync(string userLogin, IEnumerable<string> rightIds, CancellationToken ct = default)
        {
            var usersResp = await _apiClient.GetJsonAsync<UserResponse>(ApiEndpoints.UsersAll, ct).ConfigureAwait(false);
            var user = (usersResp?.data?.FirstOrDefault(u => u.login == userLogin)) ?? throw new ArgumentNullException($"Переданный логин имеет значение null! Логин не может быть null!");
            IsUserLocked(user, userLogin);
            await AddOrRemoveUserAccessAsync(_apiClient, rightIds, userLogin, HttpMethod.Put.ToString(), ct);
        }

        public async Task RemoveUserPermissionsAsync(string userLogin, IEnumerable<string> rightIds, CancellationToken ct = default)
        {
            var usersResp = await _apiClient.GetJsonAsync<UserResponse>(ApiEndpoints.UsersAll, ct).ConfigureAwait(false);
            var user = (usersResp?.data?.FirstOrDefault(u => u.login == userLogin)) ?? throw new ArgumentNullException($"Переданный логин имеет значение null! Логин не может быть null!");
            IsUserLocked(user, userLogin);
            await AddOrRemoveUserAccessAsync(_apiClient, rightIds, userLogin, HttpMethod.Delete.ToString(), ct);
        }
    }
}
