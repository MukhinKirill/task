namespace Task.Connector.DomainModels;

public static class SqlCommands
{
    public const String InsertUserQuery =
        "INSERT INTO \"TestTaskSchema\".\"User\" ( login, \"lastName\", \"firstName\", \"middleName\", \"telephoneNumber\", \"isLead\") VALUES ( @login, @lastName, @firstName, @middleName, @telephoneNumber, cast(@isLead as boolean) );";

    public const String InsertPasswordQuery =
        " INSERT INTO \"TestTaskSchema\".\"Passwords\" (\"userId\", password) VALUES ( @login, @password)";

    public const String IsUserExistQuery = "SELECT 1 FROM \"TestTaskSchema\".\"User\" WHERE login = @login";

    public const String GetAllRolesQuery = "SELECT id, name from \"TestTaskSchema\".\"ItRole\";";

    public const String GetAllRequestsRightsQuery = "SELECT id, name from \"TestTaskSchema\".\"RequestRight\";";

    public const String GetUserPropertiesQuery =
        "SELECT \"lastName\" as lastName, \"firstName\" as firstName, \"middleName\" as middleName, \"telephoneNumber\" as telephoneNumber, \"isLead\" as isLead from \"TestTaskSchema\".\"User\" WHERE \"TestTaskSchema\".\"User\".login = @login;";

    public const String GetUserRolesQuery = "select name from \"TestTaskSchema\".\"UserITRole\" uir left join \"TestTaskSchema\".\"ItRole\" ir on uir.\"roleId\" = ir.id WHERE uir.\"userId\" = @userId;";

    public const String GetUserRequestRightsQuery =
        "SELECT name from \"TestTaskSchema\".\"UserRequestRight\" urr left join \"TestTaskSchema\".\"RequestRight\" rr on rr.id = urr.\"rightId\" WHERE urr.\"userId\" = @userId;";
    
    public const String UpdateUserQuery = "UPDATE \"TestTaskSchema\".\"User\" set {0} WHERE login = @login;";

    public const String DeleteUserRequestRightsQuery =
        "DELETE from \"TestTaskSchema\".\"UserRequestRight\" WHERE \"rightId\" = ANY (@ids) and \"userId\" = @userId";

    public const String DeleteUserRolesQuery =
        "DELETE from \"TestTaskSchema\".\"UserITRole\" WHERE \"UserITRole\".\"roleId\" = ANY (@ids) and \"userId\" = @userId";

    public const String AddUserRequestRights = "INSERT INTO \"TestTaskSchema\".\"UserITRole\" (\"userId\", \"roleId\") VALUES {0};";

    public const String AddUserRoles =
        "INSERT INTO \"TestTaskSchema\".\"UserITRole\" (\"userId\", \"roleId\") VALUES {0};";
}