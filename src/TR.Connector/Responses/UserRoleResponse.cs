namespace TR.Connector.Responses;

public class UserRoleResponse
{
    public List<RoleResponseData> Datas { get; set; }
    public bool Success { get; set; }
    public object ErrorText { get; set; }
    public int Count { get; set; }
}