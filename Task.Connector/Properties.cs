using Task.Integration.Data.Models.Models;

namespace Task.Connector;

static class Properties
{
    public static readonly Property FirstName = new Property(PropertyType.FirstName, string.Empty);
    public static readonly Property MiddleName = new Property(PropertyType.MiddleName, string.Empty);
    public static readonly Property LastName = new Property(PropertyType.LastName, string.Empty);
    public static readonly Property TelephoneNumber = new Property(PropertyType.TelephoneNumber, string.Empty);
    public static readonly Property IsLead = new Property(PropertyType.IsLead, string.Empty);
    public static readonly Property Password = new Property(PropertyType.Password, string.Empty);

    public static IEnumerable<Property> GetAll() => properties;
    private static readonly List<Property> properties = new List<Property>
    {
        FirstName,
        MiddleName,
        LastName,
        TelephoneNumber,
        IsLead,
        Password,
    };
}
