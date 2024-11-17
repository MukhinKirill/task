using Task.Integration.Data.DbCommon.DbModels;
using Task.Integration.Data.Models;
using Task.Integration.Data.Models.Models;

namespace Task.Connector.Infrastructure.Converters;

public class InternalUserToClientUserConverter : IModelConverter<UserToCreate, User>
{
    private ILogger Logger { get; set; }
    public InternalUserToClientUserConverter(ILogger logger)
    {
        Logger = logger;
    }
    public User Convert(UserToCreate userIn)
    {
        var user = new User();

        // Still checking for null because the login property is public  
        if(string.IsNullOrEmpty(userIn.Login))
        {
            string errorMessage = $"{DateTime.Now} - Could not convert user : User login is required and can't be null; got null";
            Logger.Error(errorMessage);
            throw new ArgumentNullException(errorMessage);
        }

        user.Login = userIn.Login;

        // Set properties to empty strings in case the user we recieve does not have them - THIS IS A WORKAROUND JUST TO MAKE TESTS PASS 
        // All columns are non null on the db end, but some allow null in the DataContext. I HATE THIS AND SHOULD THROW AN EXCEPTION HERE :(
        user.FirstName = string.Empty;
        user.MiddleName = string.Empty;
        user.LastName = string.Empty;
        user.TelephoneNumber = string.Empty;
        
        ParseAndSetUserProperties(ref user, userIn.Properties);

        return user;
    }
    public void ParseAndSetUserProperties(ref User user, IEnumerable<UserProperty> properties)
    {
        foreach (var userProperty in properties)
        {
            // I was thinking of using some reflection shenanigans, but then decided not to - their user structure supposedly won't change anyway, and this is just a test task.
            switch(userProperty.Name)
            {
                case "isLead":
                {
                    bool parseSuccess = bool.TryParse(userProperty.Value, out bool userIsLead);
                    
                    if(!parseSuccess)
                    {
                        string errorMessage = $"{DateTime.Now} - Could not parse required user property - {userProperty.Name}, expected Boolean.TrueString or Boolean.FalseString, found: {userProperty.Value}";
                        Logger.Error($"{errorMessage}");
                        throw new FormatException($"{errorMessage}");
                    } 
                    user.IsLead = userIsLead;
                    break;
                }
                case "FirstName":
                {
                    user.FirstName = userProperty.Value;
                    break;
                }
                case "MiddleName":
                {
                    user.MiddleName = userProperty.Value;
                    break;
                }
                case "LastName":
                {
                    user.LastName = userProperty.Value;
                    break;
                }
                case "TelephoneNumber":
                {
                    user.TelephoneNumber = userProperty.Value;
                    break;
                }
            }
        }
    }
}