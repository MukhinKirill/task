using Task.Integration.Data.Models.Models;

namespace Task.Connector.Models
{
    public class UserModel
    {
        public string Login { get; protected set; } = null!;

        public string? LastName { get; protected set; }

        public string? FirstName { get; protected set; }

        public string? MiddleName { get; protected set; }

        public string? TelephoneNumber { get; protected set; }

        public bool IsLead { get; protected set; }

        public UserModel() { }

        public UserModel(string login, IEnumerable<UserProperty> userProperties)
        {
            Login = login;

            FillObject(userProperties);
        }

        public void UpdateObject(IEnumerable<UserProperty> userProperties)
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
