namespace Task.Connector.Exceptions
{
    internal class UserAlreadyExistsException : Exception
    {
        public UserAlreadyExistsException(string userLogin) : base($"User with login:{userLogin} already exists") { }
    }
}
