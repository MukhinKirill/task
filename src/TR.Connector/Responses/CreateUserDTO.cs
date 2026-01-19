namespace TR.Connector.Responses;

public class CreateUserDTO : UserPropertyData
{
    public required string Password { get; set; }
}