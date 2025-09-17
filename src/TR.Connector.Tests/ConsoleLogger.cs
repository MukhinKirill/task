using TR.Connectors.Api.Interfaces;

namespace TR.Connector.Tests
{
    internal class ConsoleLogger : ILogger
    {
        public void Debug(string message) => Console.WriteLine($"DEBUG Connector: {message}");
        public void Error(string message) => Console.WriteLine($"ERROR Connector: {message}");
        public void Warn(string message) => Console.WriteLine($"WARN Connector: {message}");
    }
}
