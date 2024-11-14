namespace Task.Connector.Exceptions;

public class ConnectorException: Exception
{
    public ConnectorException(string? message) : base(message)
    {
    }
}