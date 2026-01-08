using System.Text.Json.Serialization;

namespace TR.Connector.Models;

public class TokenResponseData
{
    [JsonPropertyName("access_token")]
    public string AccessToken { get; set; } = string.Empty;

    [JsonPropertyName("expires_in")]
    public int ExpiresIn { get; set; }
}

public class TokenResponse
{
    [JsonPropertyName("data")]
    public TokenResponseData? Data { get; set; }

    [JsonPropertyName("success")]
    public bool Success { get; set; }

    [JsonPropertyName("errorText")]
    public string? ErrorText { get; set; }

    [JsonPropertyName("count")]
    public int Count { get; set; }
}
