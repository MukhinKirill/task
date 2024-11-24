using Microsoft.EntityFrameworkCore;
using Task.Connector.Infrastructure;
using Task.Connector.Models;
using Task.Integration.Data.DbCommon.DbModels;

namespace Task.Connector.Strategies.Permission.ConcreteStrategies;

internal class RequestRightStrategy : IPermissionsStrategy
{
    private TaskDbContext _dbContext;
    public RequestRightStrategy(TaskDbContext dbContext)
    {
        _dbContext = dbContext;
    }
    public string Name => nameof(RequestRightStrategy);

    public void AddUserPermission(string userLogin, SpecifiedPermission permission)
    {
        _dbContext.UserRequestRights.Add(new UserRequestRight()
        {
            RightId = permission.Id,
            UserId = userLogin
        });
    }

    public void RemoveUserPermission(string userLogin, SpecifiedPermission permission)
    {
        var requestRightToDelete = _dbContext.UserRequestRights
                    .SingleOrDefault(urr => urr.RightId == permission.Id && urr.UserId == userLogin);

        if (requestRightToDelete is not null)
            _dbContext.UserRequestRights.Remove(requestRightToDelete);
    }
}