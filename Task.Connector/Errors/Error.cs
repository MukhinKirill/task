using Task.Integration.Data.Models;

namespace Task.Connector.Errors;

public static class Error 
{
    public static void Throw<TException>(ILogger logger, TException exception) where TException : Exception
    {
        logger.Error(exception.Message);
        throw exception;
    }

    public static void LoggerError() => throw new ArgumentNullException();
}