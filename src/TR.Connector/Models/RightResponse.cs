using System.Text.Json.Serialization;

namespace TR.Connector.Models;

public class RightResponseData
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("users")]
    public object? Users { get; set; }
}

public class RightResponse
{
    [JsonPropertyName("data")]
    public List<RightResponseData>? Data { get; set; }

    [JsonPropertyName("success")]
    public bool Success { get; set; }

    [JsonPropertyName("errorText")]
    public string? ErrorText { get; set; }

    [JsonPropertyName("count")]
    public int Count { get; set; }
}

public class UserRightResponse
{
    [JsonPropertyName("data")]
    public List<RightResponseData>? Data { get; set; }

    [JsonPropertyName("success")]
    public bool Success { get; set; }

    [JsonPropertyName("errorText")]
    public string? ErrorText { get; set; }

    [JsonPropertyName("count")]
    public int Count { get; set; }
}
