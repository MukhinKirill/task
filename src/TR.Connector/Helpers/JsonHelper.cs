using System.Text;
using System.Text.Json;

namespace TR.Connector.Helpers;

public static class JsonHelper
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public static StringContent CreateJsonContent(object value)
    {
        var json = JsonSerializer.Serialize(value);
        return new StringContent(json, Encoding.UTF8, "application/json");
    }
    
    public static T? Deserialize<T>(HttpResponseMessage response)
    {
        var json = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
        return JsonSerializer.Deserialize<T>(json, JsonOptions);
    }
}