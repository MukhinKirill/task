namespace Task.Connector.Infrastructure.Services.Logger;

public interface ILogger : Integration.Data.Models.ILogger
{
    ILogger Init(string fileName, string connectorName);
}
