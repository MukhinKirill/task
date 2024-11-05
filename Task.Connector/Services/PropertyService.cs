using Dapper;
using System.Data;
using AvanpostGelik.Connector.Interfaces;
using Task.Integration.Data.Models;
using Task.Integration.Data.Models.Models;
using Task.Connector.Interfaces;

namespace Task.Connector.Services;

public class PropertyService : IPropertyService
{
    private readonly IDatabaseService _connection;
    private readonly ILogger _logger;

    public PropertyService(IDatabaseService connection, ILogger logger)
    {
        _connection = connection;
        _logger = logger;
    }

    public IEnumerable<Property> GetAllProperties()
    {
        _logger.Debug("Fetching all properties.");

        using var connection = _connection.GetOpenConnection();
        // Получение свойств пользователя, исключая login
        var userProperties = connection.Query<string>(
            "SELECT COLUMN_NAME " +
            "FROM INFORMATION_SCHEMA.COLUMNS " +
            "WHERE TABLE_NAME = 'User' AND TABLE_SCHEMA = 'TestTaskSchema' AND COLUMN_NAME != 'login'"
        ).Select(column => new Property(column, "User Property"));

        // Получение только поля password из таблицы Passwords
        var passwordProperties = connection.Query<string>(
            "SELECT COLUMN_NAME " +
            "FROM INFORMATION_SCHEMA.COLUMNS " +
            "WHERE TABLE_NAME = 'Passwords' AND TABLE_SCHEMA = 'TestTaskSchema' AND COLUMN_NAME = 'password'"
        ).Select(column => new Property(column, "Password Property"));

        // Объединение свойств
        return userProperties.Concat(passwordProperties);
    }

    public IEnumerable<UserProperty> GetUserProperties(string userLogin)
    {
        _logger.Debug($"Fetching properties for user {userLogin}.");

        using var connection = _connection.GetOpenConnection();
        // Получение значений всех свойств пользователя из таблицы User
        return connection.Query<UserProperty>(
            "SELECT 'lastName' AS Name, lastName AS Value FROM [TestTaskSchema].[User] WHERE login = @Login UNION ALL " +
            "SELECT 'firstName', firstName FROM [TestTaskSchema].[User] WHERE login = @Login UNION ALL " +
            "SELECT 'middleName', middleName FROM [TestTaskSchema].[User] WHERE login = @Login UNION ALL " +
            "SELECT 'telephoneNumber', telephoneNumber FROM [TestTaskSchema].[User] WHERE login = @Login UNION ALL " +
            "SELECT 'isLead', CAST(isLead AS NVARCHAR) FROM [TestTaskSchema].[User] WHERE login = @Login",
            new { Login = userLogin });
    }
}