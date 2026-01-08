using System.Text.Json.Serialization;

namespace TR.Connector.Models;

public class UserPropertyData
{
    [JsonPropertyName("lastName")]
    public string LastName { get; set; } = string.Empty;

    [JsonPropertyName("firstName")]
    public string FirstName { get; set; } = string.Empty;

    [JsonPropertyName("middleName")]
    public string MiddleName { get; set; } = string.Empty;

    [JsonPropertyName("telephoneNumber")]
    public string TelephoneNumber { get; set; } = string.Empty;

    [JsonPropertyName("isLead")]
    public bool IsLead { get; set; }

    [JsonPropertyName("login")]
    public string Login { get; set; } = string.Empty;

    [JsonPropertyName("status")]
    public string Status { get; set; } = string.Empty;
}

public class UserPropertyResponse
{
    [JsonPropertyName("data")]
    public UserPropertyData? Data { get; set; }

    [JsonPropertyName("success")]
    public bool Success { get; set; }

    [JsonPropertyName("errorText")]
    public string? ErrorText { get; set; }

    [JsonPropertyName("count")]
    public int Count { get; set; }
}
