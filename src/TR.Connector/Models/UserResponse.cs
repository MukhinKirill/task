using System.Text.Json.Serialization;

namespace TR.Connector.Models;

public class UserResponseData
{
    [JsonPropertyName("login")]
    public string Login { get; set; } = string.Empty;

    [JsonPropertyName("status")]
    public string Status { get; set; } = string.Empty;
}

public class UserResponse
{
    [JsonPropertyName("data")]
    public List<UserResponseData>? Data { get; set; }

    [JsonPropertyName("success")]
    public bool Success { get; set; }

    [JsonPropertyName("errorText")]
    public string? ErrorText { get; set; }

    [JsonPropertyName("count")]
    public int Count { get; set; }
}
