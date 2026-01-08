using System.Text.Json.Serialization;

namespace TR.Connector.Models;

public class RoleResponseData
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("corporatePhoneNumber")]
    public string? CorporatePhoneNumber { get; set; }
}

public class RoleResponse
{
    [JsonPropertyName("data")]
    public List<RoleResponseData>? Data { get; set; }

    [JsonPropertyName("success")]
    public bool Success { get; set; }

    [JsonPropertyName("errorText")]
    public string? ErrorText { get; set; }

    [JsonPropertyName("count")]
    public int Count { get; set; }
}

public class UserRoleResponse
{
    [JsonPropertyName("data")]
    public List<RoleResponseData>? Data { get; set; }

    [JsonPropertyName("success")]
    public bool Success { get; set; }

    [JsonPropertyName("errorText")]
    public string? ErrorText { get; set; }

    [JsonPropertyName("count")]
    public int Count { get; set; }
}
