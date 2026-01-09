using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using TR.Connectors.Api.Entities.DTO.Requests;
using TR.Connectors.Api.Entities.DTO.Responses.UserPropertyResponse;
using TR.Connectors.Api.Entities.DTO.Responses.UserResponse;
using TR.Connectors.Api.Entities.Model;
using TR.Connectors.Api.Enums;
using TR.Connectors.Api.Interfaces;

namespace TR.Connector
{
    public class UserService: IUserService
    {
        private readonly IApiClient _apiClient;
        private readonly ILogger<UserService> _logger;

        private const string FIRST_NAME = "lastName";
        private const string LAST_NAME = "firstName";
        private const string MIDDLE_NAME = "middleName";
        private const string TEL_NUMBER = "telephoneNumber";
        private const string IS_LEAD = "isLead";
        public UserService(IApiClient apiClient, ILogger<UserService> logger)
        {
            _apiClient = apiClient ?? throw new ArgumentNullException(nameof(apiClient));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public IEnumerable<Property> GetAllProperties()
        {
            var props = new List<Property>();
            foreach (var propertyInfo in new UserPropertyData().GetType().GetProperties())
            {
                if (propertyInfo.Name == "login") continue;
                props.Add(new Property(propertyInfo.Name, propertyInfo.Name));
            }
            return props;
        }

        public async Task<IEnumerable<UserProperty>> GetUserPropertiesAsync(string userLogin, CancellationToken ct = default)
        {
            var resp = await _apiClient.GetJsonAsync<UserPropertyResponse>(ApiEndpoints.UserByLogin(userLogin), ct).ConfigureAwait(false);
            var user = resp?.data ?? throw new ArgumentNullException($"Логин пользователя не может быть null!");

            if (user.status == UserStatusType.Lock.ToString() && ct != CancellationToken.None)
            {
                _logger.LogError("Пользователь {login} залочен", userLogin);
                ct.ThrowIfCancellationRequested();
            }

            return user.GetType().GetProperties()
                .Select(p => new UserProperty(p.Name, p.GetValue(user)!.ToString()!));
        }

        public async Task UpdateUserPropertiesAsync(IEnumerable<UserProperty> properties, string userLogin, CancellationToken ct = default)
        {
            var resp = await _apiClient.GetJsonAsync<UserPropertyResponse>(ApiEndpoints.UserByLogin(userLogin), ct).ConfigureAwait(false);
            var user = resp?.data ?? throw new ArgumentNullException($"Логин пользователя не может быть null!");

            if (user.status == UserStatusType.Lock.ToString())
                throw new InvalidOperationException($"Пользователь {userLogin} залочен");

            foreach (var property in properties)
            {
                foreach (var userProp in user.GetType().GetProperties())
                {
                    if (property.Name == userProp.Name)
                        userProp.SetValue(user, property.Value);
                }
            }

            var putResponse = await _apiClient.PutJsonAsync<UserPropertyData, object>(ApiEndpoints.EditUser, user, ct).ConfigureAwait(false);
        }

        public async Task<bool> IsUserExistsAsync(string userLogin, CancellationToken ct = default)
        {
            var resp = await _apiClient.GetJsonAsync<UserResponse>(ApiEndpoints.UsersAll, ct).ConfigureAwait(false);
            var user = resp?.data?.FirstOrDefault(u => u.login == userLogin);
            return user != null;
        }

        public async Task CreateUserAsync(UserToCreate user, CancellationToken ct = default)
        {
            var dto = new CreateUserDto
            {
                login = user.Login,
                password = user.HashPassword,
                lastName = user.Properties.FirstOrDefault(p => p.Name.Equals(LAST_NAME, StringComparison.OrdinalIgnoreCase))?.Value ?? string.Empty,
                firstName = user.Properties.FirstOrDefault(p => p.Name.Equals(FIRST_NAME, StringComparison.OrdinalIgnoreCase))?.Value ?? string.Empty,
                middleName = user.Properties.FirstOrDefault(p => p.Name.Equals(MIDDLE_NAME, StringComparison.OrdinalIgnoreCase))?.Value ?? string.Empty,
                telephoneNumber = user.Properties.FirstOrDefault(p => p.Name.Equals(TEL_NUMBER, StringComparison.OrdinalIgnoreCase))?.Value ?? string.Empty,
                isLead = bool.TryParse(user.Properties.FirstOrDefault(p => p.Name.Equals(IS_LEAD, StringComparison.OrdinalIgnoreCase))?.Value ?? string.Empty, out var v) ? v : false,
                status = string.Empty
            };

            await _apiClient.PostJsonAsync<CreateUserDto, object>(ApiEndpoints.CreateUser, dto, ct).ConfigureAwait(false);
        }

        public async Task LockUserAsync(string userLogin, CancellationToken ct = default)
        {
            var resp = await _apiClient.GetJsonAsync<UserPropertyResponse>(ApiEndpoints.UserByLogin(userLogin), ct).ConfigureAwait(false);
            var user = resp?.data ?? throw new InvalidOperationException($"Пользователь {userLogin} не существует");

            if (user.status == UserStatusType.Lock.ToString())
            {
                _logger.LogDebug("Пользователь {Login} уже залочен", userLogin);
                return;
            }

            _logger.LogDebug("Блокировка пользователя {Login}", userLogin);
            using var putResp = await _apiClient.PutAsync(ApiEndpoints.LockUser(userLogin), null!, ct).ConfigureAwait(false);
            if (!putResp.IsSuccessStatusCode)
            {
                var body = await putResp.Content.ReadAsStringAsync(ct).ConfigureAwait(false);
                _logger.LogError("Залочить пользователя {Login} не удалось ({Status}): {Body}", userLogin, putResp.StatusCode, body);
                throw new HttpRequestException($"Lock failed {putResp.StatusCode}");
            }
        }

        public async Task UnlockUserAsync(string userLogin, CancellationToken ct = default)
        {
            var resp = await _apiClient.GetJsonAsync<UserPropertyResponse>(ApiEndpoints.UserByLogin(userLogin), ct).ConfigureAwait(false);
            var user = resp?.data ?? throw new InvalidOperationException($"User {userLogin} not found");

            if (user.status == UserStatusType.Unlock.ToString())
            {
                _logger.LogDebug("Пользователь {Login} уже разлочен", userLogin);
                return;
            }

            _logger.LogDebug("Разблокировка пользователя {Login}", userLogin);
            using var putResp = await _apiClient.PutAsync(ApiEndpoints.UnlockUser(userLogin), null!, ct).ConfigureAwait(false);
            if (!putResp.IsSuccessStatusCode)
            {
                var body = await putResp.Content.ReadAsStringAsync(ct).ConfigureAwait(false);
                _logger.LogError("Разблокировка пользователя {Login} не удалась ({Status}): {Body}", userLogin, putResp.StatusCode, body);
                throw new HttpRequestException($"Unlock failed {putResp.StatusCode}");
            }
        }
    }
}
