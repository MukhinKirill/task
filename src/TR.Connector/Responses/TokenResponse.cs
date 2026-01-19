namespace TR.Connector.Responses;

public class TokenResponse
{
    public TokenResponseData Data { get; set; }
    public bool Success { get; set; }
    public object ErrorText { get; set; }
    public object Count { get; set; }
}