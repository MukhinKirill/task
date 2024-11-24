namespace Task.Connector.Exceptions;

internal class InvalidUserException : Exception
{
    public InvalidUserException(string message)
    : base(message) { }
}