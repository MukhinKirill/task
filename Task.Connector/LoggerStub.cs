using Microsoft.Extensions.Logging;

namespace Task.Connector
{
	public class LoggerStub<T> : ILogger<T>
	{
		public IDisposable BeginScope<TState>(TState state) => null;

		public bool IsEnabled(LogLevel logLevel) => true;

		public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception?, string> formatter)
		{
			// Заглушка: фиксируем вызовы в консоль или делаем ничего
			Console.WriteLine($"[{logLevel}] {formatter(state, exception)}");
		}
	}
}