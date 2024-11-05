using Dapper;
using AvanpostGelik.Connector.Interfaces;
using Task.Integration.Data.Models.Models;
using Task.Connector.Interfaces;

namespace Task.Connector.Repositories;

public class UserRepository : IUserRepository
{
    private readonly IDatabaseService _databaseService;

    public UserRepository(IDatabaseService databaseService)
    {
        _databaseService = databaseService;
    }

    public void CreateUser(UserToCreate user)
    {
        _databaseService.ExecuteInTransaction((conn, transaction) =>
        {
            if (CheckUserExists(user.Login))
            {
                throw new InvalidOperationException("User already exists.");
            }

            conn.Execute(
                "INSERT INTO [TestTaskSchema].[User] (login, lastName, firstName, middleName, telephoneNumber, isLead) " +
                "VALUES (@Login, @LastName, @FirstName, @MiddleName, @TelephoneNumber, @IsLead)",
                new
                {
                    user.Login,
                    LastName = user.Properties.FirstOrDefault(p => p.Name == "lastName")?.Value ?? "DefaultLastName",
                    FirstName = user.Properties.FirstOrDefault(p => p.Name == "firstName")?.Value ?? "DefaultFirstName",
                    MiddleName = user.Properties.FirstOrDefault(p => p.Name == "middleName")?.Value ?? "DefaultMiddleName",
                    TelephoneNumber = user.Properties.FirstOrDefault(p => p.Name == "telephoneNumber")?.Value ?? "000-000-0000",
                    IsLead = user.Properties.FirstOrDefault(p => p.Name == "isLead")?.Value == "true"
                }, transaction);
        });
    }

    public bool CheckUserExists(string login)
    {
        using var connection = _databaseService.GetOpenConnection();
        return connection.QuerySingleOrDefault<bool>(
            "SELECT CASE WHEN COUNT(1) > 0 THEN 1 ELSE 0 END " +
            "FROM [TestTaskSchema].[User] WHERE login = @Login",
            new { Login = login });
    }

    public IEnumerable<UserProperty> GetUserProperties(string userLogin)
    {
        using var connection = _databaseService.GetOpenConnection();
        return connection.Query<UserProperty>(
            "SELECT 'lastName' AS Name, lastName AS Value FROM [TestTaskSchema].[User] WHERE login = @Login UNION ALL " +
            "SELECT 'firstName', firstName FROM [TestTaskSchema].[User] WHERE login = @Login UNION ALL " +
            "SELECT 'middleName', middleName FROM [TestTaskSchema].[User] WHERE login = @Login UNION ALL " +
            "SELECT 'telephoneNumber', telephoneNumber FROM [TestTaskSchema].[User] WHERE login = @Login UNION ALL " +
            "SELECT 'isLead', CAST(isLead AS NVARCHAR) FROM [TestTaskSchema].[User] WHERE login = @Login",
            new { Login = userLogin });
    }

    public void UpdateUserProperties(IEnumerable<UserProperty> properties, string userLogin)
    {
        _databaseService.ExecuteInTransaction((conn, transaction) =>
        {
            foreach (var property in properties)
            {
                conn.Execute(
                    $"UPDATE [TestTaskSchema].[User] SET {property.Name} = @Value WHERE login = @Login",
                    new { Value = property.Value, Login = userLogin }, transaction);
            }
        });
    }
}
