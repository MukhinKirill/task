namespace Task.Connector.Helpers;

public static class ProviderExtension
{
    public static string GetProvider(this string providerString)
    {
        if (providerString.Contains("PostgreSQL"))
            return "POSTGRE";
        
        if (providerString.Contains("SqlServer"))
            return "MSSQL";
        
        throw new ArgumentException("Получен неверный провайдер");
    }
}