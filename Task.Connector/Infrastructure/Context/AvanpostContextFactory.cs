using Microsoft.EntityFrameworkCore;
using Task.Integration.Data.Models;

namespace Task.Connector.Infrastructure.Context;

/// <summary>
/// Фабрика пользователей.
/// </summary>
public class AvanpostContextFactory
{
    private readonly string _connectionString;
    private readonly ILogger _logger;

    /// <summary>
    /// Конструткор фабрики контекста БД.
    /// </summary>
    /// <param name="connectionString">Строка подключения.</param>
    /// <param name="logger">Логгер.</param>
    public AvanpostContextFactory(string connectionString, ILogger logger)
    {
        _logger = logger;
        _connectionString = connectionString;
    }

    /// <summary>
    /// Получить контекст.
    /// </summary>
    /// <param name="providerName">Провайдер БД.</param>
    /// <returns>Контекст AvanpostContext.</returns>
    public AvanpostContext GetContext(string providerName)
    {
        var optionsBuilder = new DbContextOptionsBuilder<AvanpostContext>();
        switch (providerName)
        {
            case "MSSQL":
                optionsBuilder.UseSqlServer(_connectionString);
                return new AvanpostContext(optionsBuilder.Options);
            case "POSTGRE":
                optionsBuilder.UseNpgsql(_connectionString);
                return new AvanpostContext(optionsBuilder.Options);
            default:
                _logger.Error($"Invalid providerName '{providerName}'");
                throw new Exception("Неопределенный провайдер - " + providerName);
        }
    }
}