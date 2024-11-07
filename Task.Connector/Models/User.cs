namespace Task.Connector.Models
{
    public class User
    {
        public string Login { get; set; }
        public string LastName { get; set; }
        public string FirstName { get; set; }
        public string MiddleName { get; set; }
        public string TelephoneNumber { get; set; }
        public bool IsLead { get; set; }
        public User() { }
        public User(string login, string lastName, string firstName, string middleName, string telephoneNumber, bool isLead)
        {
            Login = login;
            LastName = lastName;
            FirstName = firstName;
            MiddleName = middleName;
            TelephoneNumber = telephoneNumber;
            IsLead = isLead;
        }
    }
}

