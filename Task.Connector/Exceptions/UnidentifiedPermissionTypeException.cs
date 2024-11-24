namespace Task.Connector.Exceptions;

internal class UnidentifiedPermissionTypeException : Exception
{
    public UnidentifiedPermissionTypeException(string message)
    : base(message) { }
}