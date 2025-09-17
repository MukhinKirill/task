namespace TR.Connectors.Api.Entities;

public sealed class UserToCreate
{
    public string Login { get; set; }
    public string HashPassword { get; set; }
    public IEnumerable<UserProperty> Properties { get; set; }

    public UserToCreate(string login, string hashPassword)
    {
        if (string.IsNullOrWhiteSpace(login) || string.IsNullOrWhiteSpace(hashPassword))
            throw new Exception(string.IsNullOrWhiteSpace(login) ? "login" : "password");
        HashPassword = hashPassword;
        Login = login;
        Properties = Array.Empty<UserProperty>();
    }
}