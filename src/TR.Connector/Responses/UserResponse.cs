namespace TR.Connector.Responses;

public class UserResponse
{
    public List<UserResponseData> Datas { get; set; }
    public bool Success { get; set; }
    public object ErrorText { get; set; }
    public int Count { get; set; }
}