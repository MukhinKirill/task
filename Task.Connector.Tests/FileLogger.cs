using Task.Integration.Data.Models;

namespace Task.Connector.Tests
{
    public class FileLogger : ILogger
    {
        private readonly string _fileName;
        private readonly string _connectorName;

        public FileLogger(string fileName, string connectorName)
        {
            _fileName = fileName;
            _connectorName = connectorName;
        }
        private void Append(string text)
        {
            using var sw = File.AppendText(_fileName);
            sw.Write(text);
        }
        private void AddMessage(string type, string content) => Append($"{DateTime.UtcNow}:{_connectorName}:{type}:{content}");
        public void Debug(string message) => AddMessage("DEBUG", message);
        public void Error(string message) => AddMessage("ERROR", message);
        public void Warn(string message) => AddMessage("WARNING", message);

    }
}
