using Task.Connector.ClientSchemes;
using Task.Integration.Data.Models.Models;

namespace Task.Connector.Models
{
    sealed internal class DbUser
    {
        public string Login { get; set; }
        public string FirstName { get; set; }
        public string MiddleName { get; set; }
        public string LastName { get; set; }
        public string TelephoneNumber { get; set; }
        public bool IsLead { get; set; }

        public DbUser()
        {
            Login = string.Empty;
            FirstName = string.Empty;
            MiddleName = string.Empty;
            LastName = string.Empty;
            TelephoneNumber = string.Empty;
        }

        public DbUser(UserToCreate user) : this()
        {
            Login = user.Login;
            SetProperties(user.Properties);
        }

        public UserProperty[] GetProperties()
        {
            return PropertyGettersMap
                .Select(getter => new UserProperty(getter.Key, getter.Value(this)))
                .ToArray();
        }

        public void SetProperties(IEnumerable<UserProperty> properties)
        {
            foreach (var property in properties)
            {
                PropertySettersMap[property.Name](this, property.Value);
            }
        }

        private static readonly Dictionary<string, Action<DbUser, dynamic>> PropertySettersMap = new Dictionary<string, Action<DbUser, dynamic>>()
        {
            [ClientUserScheme.firstName] = (user, value) => user.FirstName = value,
            [ClientUserScheme.middleName] = (user, value) => user.MiddleName = value,
            [ClientUserScheme.lastName] = (user, value) => user.LastName = value,
            [ClientUserScheme.telephoneNumber] = (user, value) => user.TelephoneNumber = value,
            [ClientUserScheme.isLead] = (user, value) => user.IsLead = bool.Parse(value),
        };

        private static readonly Dictionary<string, Func<DbUser, string>> PropertyGettersMap = new Dictionary<string, Func<DbUser, string>>()
        {
            [ClientUserScheme.firstName] = (user) => user.FirstName,
            [ClientUserScheme.middleName] = (user) => user.MiddleName,
            [ClientUserScheme.lastName] = (user) => user.LastName,
            [ClientUserScheme.telephoneNumber] = (user) => user.TelephoneNumber,
            [ClientUserScheme.isLead] = (user) => user.IsLead.ToString(),
        };
    }
}
