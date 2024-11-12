using Task.Integration.Data.Models;

namespace Task.Connector.Tests
{
	public class FileLogger : ILogger
	{
		readonly string _fileName;
		readonly string _connectorName;

		public FileLogger(string fileName, string connectorName)
		{
			_fileName = fileName;
			_connectorName = connectorName;
		}
		void Append(string text)
		{
			using (var sw = File.AppendText(_fileName))
				sw.WriteLine(text);
		}
		public void Debug(string message) => Append($"{DateTime.Now.ToShortDateString()}-{_connectorName}-DEBUG-{message}");
		public void Error(string message) => Append($"{DateTime.Now.ToShortDateString()}-{_connectorName}-ERROR-{message}");
		public void Warn(string message) => Append($"{DateTime.Now.ToShortDateString()}-{_connectorName}-WARNING-{message}");

	}
}
