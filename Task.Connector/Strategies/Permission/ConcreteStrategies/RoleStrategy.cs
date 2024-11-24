using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Task.Connector.Infrastructure;
using Task.Connector.Models;
using Task.Integration.Data.DbCommon.DbModels;

namespace Task.Connector.Strategies.Permission.ConcreteStrategies;

internal class RoleStrategy : IPermissionsStrategy
{
    private TaskDbContext _dbContext;
    public RoleStrategy(TaskDbContext dbContext)
    {
        _dbContext = dbContext;
    }
    public string Name => nameof(RoleStrategy);

    public void AddUserPermission(string userLogin, SpecifiedPermission permission)
    {
        _dbContext.UserITRoles.Add(new UserITRole()
        {
            UserId = userLogin,
            RoleId = permission.Id,
        });
    }

    public void RemoveUserPermission(string userLogin, SpecifiedPermission permission)
    {
        var roleToDelete = _dbContext.UserITRoles
                    .SingleOrDefault(r => r.RoleId == permission.Id && r.UserId == userLogin);

        if (roleToDelete is not null)
            _dbContext.UserITRoles.Remove(roleToDelete);
    }
}