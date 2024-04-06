using Task.Integration.Data.Models.Models;

namespace Task.Connector.Models
{
    internal class UserModel
    {
        public string Login { get; protected set; } = string.Empty;

        public string LastName { get; protected set; } = string.Empty;

        public string FirstName { get; protected set; } = string.Empty;

        public string MiddleName { get; protected set; } = string.Empty;

        public string TelephoneNumber { get; protected set; } = string.Empty;

        public bool IsLead { get; protected set; }

        protected UserModel() { }

        public UserModel(string login, IEnumerable<UserProperty> userProperties)
        {
            Login = login;

            FillObject(userProperties);
        }

        public void UpdateProperties(IEnumerable<UserProperty> userProperties)
        {
            FillObject(userProperties);
        }

        protected virtual void FillObject(IEnumerable<UserProperty> userProperties)
        {
            LastName = userProperties.FirstOrDefault(x => x.Name == "lastName")?.Value ?? LastName;
            FirstName = userProperties.FirstOrDefault(x => x.Name == "firstName")?.Value ?? FirstName;
            MiddleName = userProperties.FirstOrDefault(x => x.Name == "middleName")?.Value ?? MiddleName;

            TelephoneNumber = userProperties.FirstOrDefault(x => x.Name == "telephoneNumber")?.Value ?? TelephoneNumber;

            var isLeadValue = userProperties.FirstOrDefault(x => x.Name == "isLead")?.Value;
            var isLead = false;
            if (!string.IsNullOrEmpty(isLeadValue))
                isLead = bool.Parse(isLeadValue);

            IsLead = isLead;
        }
    }
}
