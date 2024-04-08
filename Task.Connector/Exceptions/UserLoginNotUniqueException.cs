namespace Task.Connector.Exceptions
{
    public class UserLoginNotUniqueException : Exception
    {
        public UserLoginNotUniqueException(string userLogin) : base($"User login not unique - {userLogin}")
        {
        }
    }
}
