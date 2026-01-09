using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using TR.Connectors.Api.Entities.DTO.Responses.TokenResponse;
using TR.Connectors.Api.Interfaces;

namespace TR.Connector
{
    public class AuthService: IAuthService
    {
        private readonly IApiClient _apiClient;
        private readonly ILogger<AuthService> _logger;

        public AuthService(IApiClient apiClient, ILogger<AuthService> logger)
        {
            _apiClient = apiClient ?? throw new ArgumentNullException(nameof(apiClient));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task StartUpAsync(string connectionString, CancellationToken ct = default)
        {
            var opts = ConnectionStringParser.Parse(connectionString);

            _logger.LogDebug("AuthService.StartUp: baseUrl={Url}, login={Login}", opts.Url, opts.Login);

            _apiClient.SetBaseAddress(opts.Url);

            var loginRequest = new { login = opts.Login, password = opts.Password };

            TokenResponse? tokenResp;
            try
            {
                tokenResp = await _apiClient.PostJsonAsync<object, TokenResponse>(ApiEndpoints.Login, loginRequest, ct).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Аутентификация не прошла для юзера {Login} {Url}", opts.Login, opts.Url);
                throw;
            }

            var token = tokenResp?.data?.access_token;
            if (string.IsNullOrWhiteSpace(token))
            {
                _logger.LogError("Аутентификация прошла, но отсутствует токен");
                throw new InvalidOperationException("Failed to obtain access token");
            }

            _apiClient.SetBearerToken(token);
            _logger.LogInformation("Юзер {Login} аутентифицирован", opts.Login);
        }
    }
}
