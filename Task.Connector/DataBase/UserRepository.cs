using System.Text;
using Dapper;
using Task.Connector.DataBase.Models;
using Task.Integration.Data.Models.Models;

namespace Task.Connector.DataBase;

public class UserRepository
{
    private readonly DataContext _context;

    private const string CreateUserSql = $"""
                                          INSERT INTO "TestTaskSchema"."User"
                                          (login, "lastName", "firstName", "middleName", "telephoneNumber", "isLead")
                                          VALUES (@login, @lastName, @firstName, @middleName, @telephoneNumber, @isLead)
                                          """;

    private const string CreatePasswordSql = $"""
                                           INSERT INTO "TestTaskSchema"."Passwords"
                                           (id, "userId", "password")
                                           VALUES((SELECT ((SELECT max("id") from "TestTaskSchema"."Passwords") + 1)), @userId, @password)
                                           """;

    private const string IsUserExistsSql = """
                                           SELECT COUNT(1) FROM "TestTaskSchema"."User" WHERE login = @userLogin
                                           """;

    private const string UpdatePasswordSql = """
                                             UPDATE "TestTaskSchema"."Passwords"
                                             SET password=@password
                                             WHERE userId=@userId;
                                             """;
    
    private const string GetAllPropertiesSql = $"""
                                               SELECT column_name
                                               FROM information_schema.columns
                                               WHERE (table_name = 'User' or table_name = 'Passwords') and column_name not in ('userId', 'login', 'id')
                                               """;

    private const string GetUserPropertySql = $"""
                                              SELECT "lastName", "firstName", "middleName", "telephoneNumber", "isLead"
                                              FROM "TestTaskSchema"."User"
                                              WHERE login = @userLogin
                                              """;
    
    public UserRepository(DataContext context)
    {
        _context = context;
    }

    public void CreateUser(User user, Security security)
    {
        using var connection = _context.CreateConnection();

        connection.Execute(CreateUserSql, user);
        connection.Execute(CreatePasswordSql, security);
    }

    public bool IsUserExists(string userLogin)
    {
        using var connection = _context.CreateConnection();
        
        return connection.ExecuteScalar<bool>(IsUserExistsSql, new {userLogin} );
    }

    public void UpdateUser(string userLogin, Dictionary<string, object> userProperties)
    {
        using var connection = _context.CreateConnection();
        
        var parametrs = userProperties.ToDictionary(k => k.Key, v => v.Value);
        var updateProperties = string.Join(',', userProperties.Select(x => $"\"{x.Key}\" = @{x.Key}"));

        var sql = new StringBuilder("UPDATE \"TestTaskSchema\".\"User\" SET ");
        sql.Append(updateProperties);
        sql.Append(" WHERE login = @Login");
        
        parametrs.Add("Login", userLogin);
        var dynamicParameters = new DynamicParameters(parametrs);
        
        connection.Execute(sql.ToString(), dynamicParameters);
    }

    public void UpdatePassword(string newPassword, string userLogin)
    {
        using var connection = _context.CreateConnection();
        
        connection.Execute(UpdatePasswordSql, new {newPassword, userLogin});
    }

    public IEnumerable<string> GetAllProperties()
    {
        using var connection = _context.CreateConnection();

        return connection.Query<string>(GetAllPropertiesSql).ToList();
    }
    
    public IEnumerable<UserProperty> GetUserProperties (string userLogin)
    {
        using var connection = _context.CreateConnection();
        
       var user = connection.Query<User>(GetUserPropertySql, new {userLogin}).FirstOrDefault();

       return user?.GetProperties();
    }
}