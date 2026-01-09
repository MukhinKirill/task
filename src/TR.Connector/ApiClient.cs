using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using TR.Connectors.Api.Enums;
using TR.Connectors.Api.Interfaces;

namespace TR.Connector
{
    public class ApiClient : IApiClient
    {
        private readonly ILogger<ApiClient> _logger;
        private HttpClient _httpClient;
        private readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

        private void IsSuccessStatusCodeResponse(HttpResponseMessage responseMessage, string HttpRequestType, string Uri, string body)
        {
            if (!responseMessage.IsSuccessStatusCode)
            {
                _logger.LogError("{HttpRequestType} {Uri} failed ({Status}): {Body}", HttpRequestType, Uri, responseMessage.StatusCode, body);
                throw new HttpRequestException($"Request failed {responseMessage.StatusCode}");
            }
        }
        public ApiClient(HttpClient httpClient, ILogger<ApiClient> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _httpClient = httpClient;
        }

        public void SetBaseAddress(string baseUrl) => _httpClient.BaseAddress = new Uri(baseUrl);
        public void SetBearerToken(string token) => _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue(AuthType.Bearer.ToString(), token);

        public Task<HttpResponseMessage> GetAsync(string uri, CancellationToken ct = default) => _httpClient.GetAsync(uri, ct);
        public Task<HttpResponseMessage> PostAsync(string uri, HttpContent content, CancellationToken ct = default) => _httpClient.PostAsync(uri, content, ct);
        public Task<HttpResponseMessage> PutAsync(string uri, HttpContent content, CancellationToken ct = default) => _httpClient.PutAsync(uri, content, ct);
        public Task<HttpResponseMessage> DeleteAsync(string uri, CancellationToken ct = default) => _httpClient.DeleteAsync(uri, ct);

        public async Task<T?> GetJsonAsync<T>(string uri, CancellationToken ct = default)
        {
            using var resp = await GetAsync(uri, ct).ConfigureAwait(false);
            var body = await resp.Content.ReadAsStringAsync(ct).ConfigureAwait(false);
            IsSuccessStatusCodeResponse(resp, HttpMethod.Get.ToString(), uri, body);
            return JsonSerializer.Deserialize<T>(body, _jsonOptions);
        }

        public async Task<TResponse?> PostJsonAsync<TRequest, TResponse>(string uri, TRequest dto, CancellationToken ct = default)
        {
            var content = JsonContent.Create(dto, options: _jsonOptions);
            using var resp = await PostAsync(uri, content, ct).ConfigureAwait(false);
            var body = await resp.Content.ReadAsStringAsync(ct).ConfigureAwait(false);
            IsSuccessStatusCodeResponse(resp, HttpMethod.Post.ToString(), uri, body);
            return JsonSerializer.Deserialize<TResponse>(body, _jsonOptions);
        }

        public async Task<TResponse?> PutJsonAsync<TRequest, TResponse>(string uri, TRequest dto, CancellationToken ct = default)
        {
            var content = JsonContent.Create(dto, options: _jsonOptions);
            using var resp = await PutAsync(uri, content, ct).ConfigureAwait(false);
            var body = await resp.Content.ReadAsStringAsync(ct).ConfigureAwait(false);
            IsSuccessStatusCodeResponse(resp, HttpMethod.Put.ToString(), uri, body);
            return JsonSerializer.Deserialize<TResponse>(body, _jsonOptions);
        }
        public async Task<TResponse?> DeleteJsonAsync<TResponse>(string uri, CancellationToken ct = default)
        {
            using var resp = await DeleteAsync(uri, ct).ConfigureAwait(false);
            var body = await resp.Content.ReadAsStringAsync(ct).ConfigureAwait(false);
            IsSuccessStatusCodeResponse(resp, HttpMethod.Delete.ToString(), uri, body);
            return JsonSerializer.Deserialize<TResponse>(body, _jsonOptions);
        }
    }
}
