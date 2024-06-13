
using FluentResults;
using Task.Integration.Data.Models.Models;

namespace Task.Connector.Storage;

public interface IGetUserProperties
{
     string UserLogin { get; }
     IEnumerable<UserProperty> Properties { get; }
}

public interface IGetUserExist
{
     string UserLogin { get; }
}

public interface IUserRepository : IDisposable
{
     Task<Result> CreateUserAsync(UserToCreate request, CancellationToken token);
     Task<Result<bool>> DoesUserExist(IGetUserExist req, CancellationToken token);
     Task<Result<IEnumerable<Property>>> GetAllPropertiesAsync(CancellationToken token);
     Task<Result<IEnumerable<UserProperty>>> GetUserPropertiesAsync(IGetUserProperties req, CancellationToken token);
     Task<Result> UpdateUserProperties(IGetUserProperties req, CancellationToken token);
}