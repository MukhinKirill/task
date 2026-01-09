using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TR.Connectors.Api.Interfaces
{
    public interface IApiClient
    {
        void SetBaseAddress(string baseUrl);
        void SetBearerToken(string token);
        Task<HttpResponseMessage> GetAsync(string uri, CancellationToken ct = default);
        Task<HttpResponseMessage> PostAsync(string uri, HttpContent content, CancellationToken ct = default);
        Task<HttpResponseMessage> PutAsync(string uri, HttpContent content, CancellationToken ct = default);
        Task<HttpResponseMessage> DeleteAsync(string uri, CancellationToken ct = default);

        Task<T?> GetJsonAsync<T>(string uri, CancellationToken ct = default);
        Task<TResponse?> PostJsonAsync<TRequest, TResponse>(string uri, TRequest dto, CancellationToken ct = default);
        Task<TResponse?> PutJsonAsync<TRequest, TResponse>(string uri, TRequest dto, CancellationToken ct = default);
        Task<TResponse?> DeleteJsonAsync<TResponse>(string uri, CancellationToken ct = default);
    }
}
