namespace Task.Connector.Services.UserPermission
{
    public interface IUserPermission
    {
        public void AddUserPermissions(string userLogin, IEnumerable<string> rightIds);
        public void RemoveUserPermissions(string userLogin, IEnumerable<string> rightIds);
        public IEnumerable<string> GetUserPermissions(string userLogin);
    }
}