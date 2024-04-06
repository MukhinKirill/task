using Task.Integration.Data.Models.Models;

namespace Task.Connector.Models
{
    internal class UserObjectPropertyModel : UserModel
    {
        public string? Password { get; set; }

        public IReadOnlyCollection<UserProperty> GetProperties()
        {
            var properties = new List<UserProperty>();
            if (!string.IsNullOrEmpty(LastName))
                properties.Add(new UserProperty("lastName", LastName));

            if (!string.IsNullOrEmpty(FirstName))
                properties.Add(new UserProperty("firstName", FirstName));

            if (!string.IsNullOrEmpty(MiddleName))
                properties.Add(new UserProperty("middleName", MiddleName));

            if (!string.IsNullOrEmpty(TelephoneNumber))
                properties.Add(new UserProperty("telephoneNumber", TelephoneNumber));

            properties.Add(new UserProperty("isLead", IsLead.ToString()));

            return properties;
        }

        protected override void FillObject(IEnumerable<UserProperty> userProperties)
        {
            Password = userProperties.FirstOrDefault(x => x.Name == "password")?.Value ?? Password;

            base.FillObject(userProperties);
        }
    }
}
