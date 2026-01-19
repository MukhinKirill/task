namespace TR.Connector.Responses;

public class UserRightResponse
{
    public List<RightResponseData> Datas { get; set; }
    public bool Success { get; set; }
    public object ErrorText { get; set; }
    public int Count { get; set; }
}