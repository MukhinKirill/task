using System.ComponentModel.DataAnnotations.Schema;
using Task.Integration.Data.Models.Models;

namespace Task.Connector.DataBase.Models;

public class User
{
    // public User(string login, string lastName = "", string firstName = "", string middleName = "", 
    //     string telephoneNumber = "", bool isLead = default)
    // {
    //     Login = login;
    //     LastName = lastName;
    //     FirstName = firstName;
    //     MiddleName = middleName;
    //     TelephoneNumber = telephoneNumber;
    //     IsLead = isLead;
    // }

    public User() { }

    public User(string login, IEnumerable<UserProperty> userProperties)
    {
        Login = login;
        var dictionary = userProperties.ToDictionary(k => k.Name, v => v.Value);
        
        var properties = GetType().GetProperties();

        foreach (var property in properties)
        {
            if (dictionary.TryGetValue(property.Name, out var value))
            {
                property.SetValue(this,  Convert.ChangeType(value, property.PropertyType)); 
            }
        }
    }

    public IEnumerable<UserProperty> GetProperties()
    {
        var properties = new List<UserProperty>()
        {
            new ( "LastName", this.LastName ),
            new ( "FirstName", FirstName ),
            new ("MiddleName", MiddleName ),
            new ( "TelephoneNumber", TelephoneNumber ),
            new ( "IsLead", IsLead.ToString() )
        };

        return properties;
    }
    
    public string Login { get; set; }

    public string LastName { get; set; } = "";
 
    public string FirstName { get; set; } = "";

    public string MiddleName { get;  set; } = "";

    public string TelephoneNumber { get;  set; } = "";

    public bool IsLead { get; set; } = default;
}