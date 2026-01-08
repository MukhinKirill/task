namespace TR.Connector.Models;

public class ApiResponse<T>
{
    public T? Data { get; set; }
    public bool Success { get; set; }
    public string? ErrorText { get; set; }
    public int Count { get; set; }
}
