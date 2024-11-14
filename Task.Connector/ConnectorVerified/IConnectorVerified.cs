using Task.Integration.Data.Models.Models;

namespace Task.Connector.ConnectorVerified
{
    interface IConnectorVerified
    {
        public void StartUp(ref string connectionString, in ConnectorDb connector);
		public void TurnOff();
		public void CreateUser(ref UserToCreate user);
        public IEnumerable<Property> GetAllProperties();
        public IEnumerable<UserProperty> GetUserProperties(ref string userLogin);
        public bool IsUserExists(ref string userLogin);
        public void UpdateUserProperties(ref IEnumerable<UserProperty> properties, ref string userLogin);
        public IEnumerable<Permission> GetAllPermissions();
        public void AddUserPermissions(ref string userLogin, ref List<Right> rights);
        public void RemoveUserPermissions(ref string userLogin, ref List<Right> rights);
        public IEnumerable<string> GetUserPermissions(ref string userLogin);
    }
}