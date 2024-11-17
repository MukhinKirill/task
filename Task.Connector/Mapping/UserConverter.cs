using AutoMapper;
using Task.Integration.Data.DbCommon.DbModels;
using Task.Integration.Data.Models.Models;

namespace Task.Connector.Mapping;

internal class UserConverter : ITypeConverter<UserToCreate, User>
{
    public User Convert(UserToCreate userSource, User userDestination, ResolutionContext context)
    {
        var properties = userSource.Properties.ToDictionary(x => x.Name, x => x.Value);

        userDestination = new User()
        {
            Login = userSource.Login,
            LastName = properties.GetValueOrDefault("LastName") ?? string.Empty,
            FirstName = properties.GetValueOrDefault("FirstName") ?? string.Empty,
            MiddleName = properties.GetValueOrDefault("MiddleName") ?? string.Empty,
            TelephoneNumber = properties.GetValueOrDefault("TelephoneNumber") ?? string.Empty,
            IsLead = bool.Parse(properties.GetValueOrDefault("isLead") ?? "false")
        };

        return userDestination;
    }
}