namespace Task.Connector.Exceptions
{
    internal class UserNotFoundException : Exception
    {
        public UserNotFoundException(string userLogin) : base($"User not found by login - {userLogin}") { }
    }
}
