namespace Task.Connector.Infrastructure.Services.Logger;

public class FileLogger : ILogger
{
    private string _fileName = null!;
    private string _connectorName = null!;

    public ILogger Init(string fileName, string connectorName)
    {
        _fileName = fileName;
        _connectorName = connectorName;
        return this;
    }

    private void Append(string text)
    {
        Console.WriteLine(text);
        using var sw = File.AppendText(_fileName);
        sw.WriteLine(text);
    }

    public void Debug(string message) => Append($"{DateTime.Now}:{_connectorName}:DEBUG:{message}");

    public void Error(string message) => Append($"{DateTime.Now}:{_connectorName}:ERROR:{message}");

    public void Warn(string message) => Append($"{DateTime.Now}:{_connectorName}:WARNING:{message}");
}
