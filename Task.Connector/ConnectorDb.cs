using AvanpostGelik.Connector.Interfaces;
using AvanpostGelik.Connector.Repositories;
using Task.Integration.Data.Models;
using Task.Integration.Data.Models.Models;
using Task.Connector.Interfaces;
using Ardalis.GuardClauses;
using Task.Connector.Repositories;
using Task.Connector.Services;

namespace Task.Connector;

public class ConnectorDb : IConnector
{
    private readonly IUserRepository _userRepository;
    private readonly IPermissionRepository _permissionRepository;
    private readonly IPropertyService _propertyService;
    public ILogger Logger { get; set; }
    public ConnectorDb(IDatabaseService databaseService, ILogger logger)
    {
        Guard.Against.Null(databaseService, nameof(databaseService));
        Guard.Against.Null(logger, nameof(logger));
        Logger = logger;
        _userRepository = new UserRepository(databaseService);
        _permissionRepository = new PermissionRepository(databaseService);
        _propertyService = new PropertyService(databaseService, Logger);
    }

    public void StartUp(string connectionString)
    {
    }

    public void CreateUser(UserToCreate user)
    {
        Logger.Debug("Starting user creation process.");

        _userRepository.CreateUser(user);

        Logger.Debug("User created successfully.");
    }

    public IEnumerable<Property> GetAllProperties()
    {
        Logger.Debug("Fetching all properties.");

        return _propertyService.GetAllProperties();
    }

    public IEnumerable<UserProperty> GetUserProperties(string userLogin)
    {
        Logger.Debug($"Fetching properties for user {userLogin}.");
        return _userRepository.GetUserProperties(userLogin);
    }

    public bool IsUserExists(string userLogin)
    {
        Logger.Debug($"Checking if user {userLogin} exists.");
        return _userRepository.CheckUserExists(userLogin);
    }

    public void UpdateUserProperties(IEnumerable<UserProperty> properties, string userLogin)
    {
        Logger.Debug($"Updating properties for user {userLogin}.");

        _userRepository.UpdateUserProperties(properties, userLogin);

        Logger.Debug("User properties updated successfully.");
    }

    public IEnumerable<Permission> GetAllPermissions()
    {
        Logger.Debug("Fetching all permissions.");
        return _permissionRepository.GetAllPermissions();
    }

    public void AddUserPermissions(string userLogin, IEnumerable<string> rightIds)
    {
        Logger.Debug($"Adding permissions for user {userLogin}.");
        _permissionRepository.AddUserPermissions(userLogin, rightIds);
        Logger.Debug("Permissions added successfully.");
    }

    public void RemoveUserPermissions(string userLogin, IEnumerable<string> rightIds)
    {
        Logger.Debug($"Removing permissions for user {userLogin}.");
        _permissionRepository.RemoveUserPermissions(userLogin, rightIds);
        Logger.Debug("Permissions removed successfully.");
    }

    public IEnumerable<string> GetUserPermissions(string userLogin)
    {
        Logger.Debug($"Fetching permissions for user {userLogin}.");
        return _permissionRepository.GetUserPermissions(userLogin);
    }
}