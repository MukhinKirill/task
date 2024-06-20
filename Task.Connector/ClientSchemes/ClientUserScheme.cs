namespace Task.Connector.ClientSchemes
{
    internal class ClientUserScheme
    {
        public const string login = "login";
        public const string firstName = "firstName";
        public const string middleName = "middleName";
        public const string lastName = "lastName";
        public const string telephoneNumber = "telephoneNumber";
        public const string isLead = "isLead";
        public const string password = "password";

        public static readonly string[] PropertyFields = new[]
        {
            firstName,
            middleName,
            lastName,
            telephoneNumber,
            isLead,
            password,
        };
    }
}
