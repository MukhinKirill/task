using System.Runtime.CompilerServices;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Task.Connector.ComponentRegistrar;
using Task.Connector.DataAccess;
using Task.Connector.Infrastructure.Common;
using Task.Connector.Infrastructure.Services.Logger;
using Task.Integration.Data.Models;

namespace Task.Connector.DependencyInjection;

public static class ServiceLocator
{
    private static ILogger _logger = null!;
    private static string _connectionString = null!;
    private static IServiceProvider _serviceProvider = null!;

    [ModuleInitializer]
    internal static void Initialize()
    {
        _logger = new FileLogger();
        _logger.Debug($"Старт инициализации{Delimiter.Trace}ServiceLocator.Initialize()");

        _connectionString = GetConnectionString();
        var services = new ServiceCollection();
        services.AddDbContext<ConnectorDbContext>(options => options.UseNpgsql(_connectionString));
        services.AddApplicationService();

        _serviceProvider = services.BuildServiceProvider();
        _logger.Debug($"Конец инициализации{Delimiter.Trace}ServiceLocator.Initialize()");
    }

    public static void SetConnectionString(string? connectionString)
    {
        _logger.Debug($"Изменение строки подключения{Delimiter.Trace}ServiceLocator.SetConnectionString(connectionString)");
        if (!string.IsNullOrWhiteSpace(connectionString))
        {
            if (IsValidConnectionString(connectionString))
            {
                _connectionString = connectionString;
                _logger.Debug($"Строка подключения изменена" +
                    $"{Delimiter.Trace}ServiceLocator.SetConnectionString(connectionString)" +
                    $"{Delimiter.Value}connectionString=\"{_connectionString}\"");
            }
        }
        else
        {
            _logger.Warn($"Строка подключения пуста{Delimiter.Trace}ServiceLocator.SetConnectionString(connectionString)");
        }
    }

    private static bool IsValidConnectionString(string connectionString)
    {
        try
        {
            _logger.Debug($"Старт проверки строки подключения{Delimiter.Trace}ServiceLocator.IsValidConnectionString(connectionString)");
            var options = new DbContextOptionsBuilder().UseNpgsql(connectionString).Options;
            using var context = new ConnectorDbContext(options);
            _logger.Debug($"Проверка строки подключения завершена{Delimiter.Trace}ServiceLocator.IsValidConnectionString(connectionString)");

            return true;
        }
        catch (Exception)
        {
            _logger.Error($"Не удалось создать подключение с новой строкой подключения" +
                $"{Delimiter.Trace}ServiceLocator.IsValidConnectionString(connectionString)" +
                $"{Delimiter.Value}connectionString=\"{connectionString}\"");
            return false;
        }
    }

    private static string GetConnectionString()
    {
        var builder = new ConfigurationBuilder();
        builder.SetBasePath(Directory.GetCurrentDirectory());
        builder.AddJsonFile("appsettings.json");

        return builder.Build().GetConnectionString("POSTGRE")!;
    }

    public static T GetService<T>()
    {
        var service = _serviceProvider.GetService<T>();
        return service ?? throw new ArgumentNullException();
    }

    public static T GetService<T>(params object[] parameters)
    {
        var service = (T)ActivatorUtilities.CreateInstance(_serviceProvider, typeof(T), parameters);
        return service ?? throw new ArgumentNullException();
    }
}
