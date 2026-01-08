using System.Text.Json.Serialization;

namespace TR.Connector.Models;

public class CreateUserDto
{
    [JsonPropertyName("login")]
    public string Login { get; set; } = string.Empty;

    [JsonPropertyName("password")]
    public string Password { get; set; } = string.Empty;

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

    [JsonPropertyName("status")]
    public string Status { get; set; } = string.Empty;
}
