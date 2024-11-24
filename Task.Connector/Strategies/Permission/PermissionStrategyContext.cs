using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Task.Connector.Exceptions;
using Task.Connector.Infrastructure;
using Task.Connector.Models;
using Task.Connector.Strategies.Permission.ConcreteStrategies;

namespace Task.Connector.Strategies.Permission;
internal class PermissionStrategyContext
{
    SpecifiedPermission _currentPermission;
    string _userLogin;
    TaskDbContext _dbContext;

    Dictionary<string, IPermissionsStrategy> _strategyContext = new();

    public PermissionStrategyContext(SpecifiedPermission currentPermission, string userLogin, TaskDbContext dbContext)
    {
        _currentPermission = currentPermission;
        _dbContext = dbContext;
        _userLogin = userLogin;

        _strategyContext.Add(nameof(RequestRightStrategy), new RequestRightStrategy(_dbContext));
        _strategyContext.Add(nameof(RoleStrategy), new RoleStrategy(_dbContext));
    }

    public void ApplyAddStrategy(IPermissionsStrategy strategy)
    {
        strategy.AddUserPermission(_userLogin, _currentPermission);
    }

    public void ApplyRemoveStrategy(IPermissionsStrategy strategy)
    {
        strategy.RemoveUserPermission(_userLogin, _currentPermission);
    }

    public IPermissionsStrategy GetStrategy(SpecifiedPermission currentPermission)
    {
        if (currentPermission.Type == "Request")
            return _strategyContext[nameof(RequestRightStrategy)];
        else if (currentPermission.Type == "Role")
            return _strategyContext[nameof(RoleStrategy)];
        else
            throw new UnidentifiedPermissionTypeException($"Неизвестный тип разрешений: {currentPermission.Type}");
    }
}