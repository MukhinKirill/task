namespace Task.Connector.Helpers;

public static class CaseConverter
{
    public static string CamelToPascal(string camelCaseString) =>
        char.ToUpper(camelCaseString[0]) + camelCaseString[1..];

    public static string PascalToCamel(string pascalCaseString) =>
        char.ToLower(pascalCaseString[0]) + pascalCaseString[1..];
}