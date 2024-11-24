using AutoMapper;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Text.RegularExpressions;
using Task.Connector.Exceptions;
using Task.Connector.Infrastructure;
using Task.Connector.Models;
using Task.Connector.Strategies.Permission;
using Task.Connector.Validation;
using Task.Integration.Data.DbCommon.DbModels;
using Task.Integration.Data.Models;
using Task.Integration.Data.Models.Models;

namespace Task.Connector;

public class ConnectorDb : IConnector
{
    private TaskDbContext _context;
    private IMapper _mapper;
    private IValidator<User> _userValidator;
    private IValidator<Password> _passwordValidator;

    public void StartUp(string connectionString)
    {
        var actualConnectionString = GetActualConnectionString(connectionString);

        var serviceCollection = new ServiceCollection();
        serviceCollection.AddServices(actualConnectionString);

        var serviceProvider = serviceCollection.BuildServiceProvider();

        _mapper = serviceProvider.GetRequiredService<IMapper>();
        _context = serviceProvider.GetRequiredService<TaskDbContext>();
        _userValidator = serviceProvider.GetRequiredService<IValidator<User>>();
        _passwordValidator = serviceProvider.GetRequiredService<IValidator<Password>>();
    }

    public void CreateUser(UserToCreate userToCreate)
    {
        if (IsUserExists(userToCreate.Login))
            throw new InvalidUserException($"Пользователь {userToCreate.Login} уже существует в системе");

        var newUser = _mapper.Map<User>(userToCreate);

        var newUsersPassword = new Password(userToCreate.Login, userToCreate.HashPassword);
        
        if (!_userValidator.Validate(newUser).IsValid)
        {
            Logger.Error($"Невозможно создать пользователя с логином {userToCreate.Login}");
            throw new ValidationException("Некорректная информация для создания пользователя");
        }

        if (!_passwordValidator.Validate(newUsersPassword).IsValid)
        {
            Logger.Error("Пароль не отвечает требоваиям системы");
            throw new ValidationException("Некорректный пароль");
        }

        _context.Users.Add(newUser);
        _context.Passwords.Add(newUsersPassword);
        _context.SaveChanges();
    }

    public IEnumerable<Property> GetAllProperties()
    {
        var type = typeof(User);
        var userProperties = new List<Property>();
        foreach (var property in type.GetProperties())
        {
            userProperties.Add(new Property(property.Name, property.PropertyType.ToString()));
        }

        return userProperties.AsEnumerable();
    }

    public IEnumerable<UserProperty> GetUserProperties(string userLogin)
    {
        var user = _context.Users.AsNoTracking().SingleOrDefault(u => u.Login == userLogin)
            ?? throw new NotFoundException($"Пользователь с логином {userLogin} не найден");

        var properties = _mapper.Map<List<UserProperty>>(user);
        return properties;
    }

    public bool IsUserExists(string userLogin) => _context.Users.AsNoTracking().Any(u => u.Login == userLogin);

    public void UpdateUserProperties(IEnumerable<UserProperty> propertiesToUpdate, string userLogin)
    {
        var user = _context.Users.AsNoTracking().SingleOrDefault(u => u.Login == userLogin)
            ?? throw new NotFoundException($"Пользователь с логином {userLogin} не найден");

        var properties = propertiesToUpdate.ToDictionary(p => p.Name, p => p.Value);

        var userType = user.GetType();

        foreach(var userProperty in userType.GetProperties())
        {
            if (properties.ContainsKey(userProperty.Name))
                userProperty.SetValue(user, properties[userProperty.Name]);
        }

        if (!_userValidator.Validate(user).IsValid)
        {
            Logger.Error($"Ошибка обновления атрибутов пользователя {userLogin}. Данные пользователя некорректны");
            throw new ValidationException("Предоставлена некорректная информация для обновления атрибутов пользователя");
        }

        _context.Users.Update(user);
        _context.SaveChanges();

        Logger.Debug($"Пользователь {userLogin} успешно обновлен");
    }

    public IEnumerable<Permission> GetAllPermissions()
    {
        var allRequestRights = _context.RequestRights.AsNoTracking().ToList();
        var allItRoles = _context.Roles.AsNoTracking().ToList();

        var permissions = new List<Permission>();
        if (allRequestRights.Any())
            permissions = _mapper.Map<List<Permission>>(allRequestRights);
        
        if(allItRoles.Any())
            permissions.AddRange(_mapper.Map<List<Permission>>(allItRoles));

        return permissions;
    }

    public void AddUserPermissions(string userLogin, IEnumerable<string> rightIds)
    {
        if (!IsUserExists(userLogin))
            throw new NotFoundException($"Пользователь с логином {userLogin} не найден");
        
        foreach (var rightId in rightIds)
        {
            var currentPermission = GetPermissionInfo(rightId);

            if (SpecifiedPermissionExists(currentPermission))
            {
                var strategyContext = new PermissionStrategyContext(currentPermission, userLogin, _context);
                var strategy = strategyContext.GetStrategy(currentPermission);
                strategyContext.ApplyAddStrategy(strategy);
            }
        }

        _context.SaveChanges();
    }

    public void RemoveUserPermissions(string userLogin, IEnumerable<string> rightIds)
    {
        if (!IsUserExists(userLogin))
            throw new NotFoundException($"Пользователь с логином {userLogin} не найден");

        foreach (var rightId in rightIds)
        {
            var currentPermission = GetPermissionInfo(rightId);

            var strategyContext = new PermissionStrategyContext(currentPermission, userLogin, _context);
            var strategy = strategyContext.GetStrategy(currentPermission);
            strategyContext.ApplyRemoveStrategy(strategy);     
        }

        _context.SaveChanges();
    }

    public IEnumerable<string> GetUserPermissions(string userLogin)
    {
        if (!IsUserExists(userLogin))
            throw new NotFoundException($"Пользователь с логином {userLogin} не найден");

        var userPermissions = new List<string>();
        var userRightIds = _context.UserRequestRights.AsNoTracking()
            .Where(urr => urr.UserId == userLogin)
            .Select(urr => urr.UserId)
            .ToList();

        userPermissions.AddRange(userRightIds);

        return userPermissions;
    }

    public ILogger Logger { get; set; }

    string GetActualConnectionString(string connectionString)
    {
        string connectionStringPattern = @"ConnectionString='([^']*)'";
        var match = Regex.Match(connectionString, connectionStringPattern);
        return match.Groups[1].Value;
    }

    bool SpecifiedPermissionExists(SpecifiedPermission permission)
    {
        var exists = false;

        if (permission.Type == "Request")
        {
            exists = _context.RequestRights.Any(rr => rr.Id == permission.Id);
        }
        else if (permission.Type == "Role")
        {
            exists = _context.Roles.Any(r => r.Id == permission.Id);
        }
        
        return exists;
    }

    SpecifiedPermission GetPermissionInfo(string rightId)
    {
        var typeAndId = rightId.Split(':');
        return new SpecifiedPermission(typeAndId[0], typeAndId[1]);
    }
}