using Task.Integration.Data.DbCommon.DbModels;
using Task.Integration.Data.Models.Models;

namespace Task.Connector;

public static class UserToCreateExtension
{
    public static User CreateUser(this UserToCreate userToCreate)
    {
        return new User
        {
            Login = userToCreate.Login,
            
            FirstName = userToCreate.Properties
                .FirstOrDefault(p=>p.Name == "FirstName")?.Value ?? string.Empty,
            
            LastName = userToCreate.Properties
                .FirstOrDefault(p=>p.Name == "LastName")?.Value ?? string.Empty,
            
            MiddleName = userToCreate.Properties
                .FirstOrDefault(p=>p.Name == "MiddleName")?.Value ?? string.Empty,
            
            IsLead = userToCreate.Properties
                .FirstOrDefault(p=>p.Name == "IsLead")?.Value == "true",
            
            TelephoneNumber = userToCreate.Properties
                .FirstOrDefault(p=>p.Name == "TelephoneNumber")?.Value ?? string.Empty
        };
    }
}