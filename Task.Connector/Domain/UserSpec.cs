
using Task.Connector.Storage;
using Task.Integration.Data.Models.Models;

namespace Task.Connector.Domain;

public sealed class GetUserExistDto : IGetUserExist
{
    public GetUserExistDto(string login)
    {
        UserLogin = login;
    }

    public string UserLogin { get; }
}

public sealed class GetGetUserPropertiesDto : IGetUserProperties
{
    public GetGetUserPropertiesDto(string userLogin)
    {
        UserLogin = userLogin;
    }
    
    public string UserLogin { get; }
    public IEnumerable<UserProperty> Properties => Array.Empty<UserProperty>();
}

public sealed class UpdateGetUserPropertiesDto : IGetUserProperties
{
    public UpdateGetUserPropertiesDto(string userLogin, IEnumerable<UserProperty> props)
    {
        UserLogin = userLogin;
        Properties = props;
    }
    public string UserLogin { get; }
    public IEnumerable<UserProperty> Properties { get; }
}