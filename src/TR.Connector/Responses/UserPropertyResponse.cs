namespace TR.Connector.Responses;

public class UserPropertyResponse
{
    public UserPropertyData Data { get; set; }
    public bool Success { get; set; }
    public object ErrorText { get; set; }
    public int Count { get; set; }
}