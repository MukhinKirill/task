namespace Task.Connector.Common.Exceptions;
public sealed class UserNotFoundException : Exception
{
    public UserNotFoundException(string login) : base($"User with login '{login}' not found")
    {

    }
}
