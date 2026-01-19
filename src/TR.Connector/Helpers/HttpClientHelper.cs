using System.Net.Http.Headers;
using TR.Connector.Responses;

namespace TR.Connector.Helpers;

public class HttpClientHelper
{
    private readonly HttpClient _httpClient = new();
    private readonly string _login;
    private readonly string _password;
    
    private string _token = string.Empty;
    private int _expiresIn;
    private DateTimeOffset _receiveTokenTime;

    public HttpClientHelper(string url, string login, string password)
    {
        _login = login;
        _password = password;
        _httpClient.BaseAddress = new Uri(url);
    }
    
    public HttpResponseMessage Get(string requestUrl)
    {
        EnsureToken();
        
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _token);
        var response = _httpClient.GetAsync(requestUrl).GetAwaiter().GetResult();
        _httpClient.DefaultRequestHeaders.Clear();

        return response;
    }

    public void Put(string requestUrl, HttpContent? content)
    {
        EnsureToken();

        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _token);
        _httpClient.PutAsync(requestUrl, content).GetAwaiter().GetResult();
        _httpClient.DefaultRequestHeaders.Clear();
    }

    public void Delete(string requestUrl)
    {
        EnsureToken();

        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _token);
        _httpClient.DeleteAsync(requestUrl).GetAwaiter().GetResult();
        _httpClient.DefaultRequestHeaders.Clear();
    }

    public void Post(string requestUrl, HttpContent? content)
    {
        EnsureToken();

        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _token);
        _httpClient.PostAsync(requestUrl, content).GetAwaiter().GetResult();
        _httpClient.DefaultRequestHeaders.Clear();
    }
    
    private void EnsureToken()
    {
        if (string.IsNullOrEmpty(_token) || !IsTokenValid())
        {
            Authorize();
        }
    }

    private void Authorize()
    {
        var body = new { login = _login, password = _password };
        var content = JsonHelper.CreateJsonContent(body);
        var response = _httpClient.PostAsync("api/v1/login", content).GetAwaiter().GetResult();
        var tokenResponse = JsonHelper.Deserialize<TokenResponse>(response);
        _token = tokenResponse?.Data.AccessToken ?? throw new InvalidOperationException("Токен аутентификации не получен");
        _expiresIn = tokenResponse.Data.ExpiresIn;
        _receiveTokenTime = DateTime.Now;
    }
    
    private bool IsTokenValid()
    {
        var currentTime = DateTime.Now;
        var expirationTime = _receiveTokenTime.AddSeconds(_expiresIn - 30);
        return expirationTime > currentTime;
    }
}