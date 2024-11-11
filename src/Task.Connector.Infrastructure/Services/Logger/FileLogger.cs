using System.Diagnostics;
using Task.Integration.Data.Models;

namespace Task.Connector.Infrastructure.Services.Logger;

public class FileLogger : ILogger
{
    private readonly string _fileName;
    private readonly string _connectorName;

    public FileLogger()
    {
        _fileName = $"{DateTime.Now: dd.MM.yyyy}_connector_POSTGRE.Log";
        _connectorName = "Connector:[POSTGRE]:";
    }

    public FileLogger(string fileName, string connectorName)
    {
        _fileName = fileName;
        _connectorName = connectorName;
    }

    private void Append(string text, ConsoleColor color)
    {
        var defaultColor = Console.ForegroundColor;
        Console.ForegroundColor = color;
        Console.WriteLine(text);
        Console.ForegroundColor = defaultColor;

        using var streamWriter = File.AppendText(_fileName);
        streamWriter.WriteLine(text);
    }

    public void Debug(string message) => RunWhenDebugging(message);

    public void Error(string message) => Append($"[{DateTime.Now}]{null,2}[ERROR]{null,4}{_connectorName}{message}", ConsoleColor.Red);

    public void Warn(string message) => Append($"[{DateTime.Now}]{null,2}[WARNING]{null,2}{_connectorName}{message}", ConsoleColor.Yellow);

    [Conditional("DEBUG")]
    private void RunWhenDebugging(string message) => Append($"[{DateTime.Now}]{null,2}[DEBUG]{null,4}{_connectorName}{message}", ConsoleColor.Green);
}
