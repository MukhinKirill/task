using Microsoft.EntityFrameworkCore;
using Task.Connector.Connectors;
using Task.Connector.Interfaces;
using Task.Connector.Models;
using Task.Connector.Tests.Constants;
using Task.Integration.Data.Models;
using Testcontainers.PostgreSql;


namespace Task.Connector.Tests
{
    public class DatabaseFixture : IAsyncLifetime
    {
        public readonly PostgreSqlContainer PostgreSqlContainer;

        public Dictionary<string, string> ConnectorsCS { get; private set; }
        public Dictionary<string, string> DataBasesCS { get; private set; }

        IConnectorDb _connector;

        string _mssqlConnectionString;
        string _postgreConnectionString;

        public DatabaseFixture()
        {
            PostgreSqlContainer = new PostgreSqlBuilder()
                .WithImage("postgres:latest")
                .Build();
        }

        public IConnector GetConnector(string provider)
        {
            _connector = new ConnectorDb
            {
                Logger = new FileLogger($"{DateTime.Now:dd.MM.yyyy}connector{provider}.Log", $"{DateTime.Now:dd.MM.yyyy}connector{provider}")
            };
            _connector.StartUp(ConnectorsCS[provider]);
            return _connector;
        }

        async System.Threading.Tasks.Task IAsyncLifetime.InitializeAsync()
        {
            await InitializePostgreAsync();

            _mssqlConnectionString = "Server=(LocalDb)\\MSSQLLocalDB;Database=testDb;Trusted_Connection=True;";
            _postgreConnectionString = PostgreSqlContainer.GetConnectionString();
            ConnectorsCS = new Dictionary<string, string>
            {
                { DatabaseConnectors.MSSQL_PROVIDER, DatabaseConnectors.GetMssqlConfiguration(_mssqlConnectionString) },
                { DatabaseConnectors.POSGRE_PROVIDER,  DatabaseConnectors.GetPostgresConfiguration(_postgreConnectionString)}
            };

            DataBasesCS = new Dictionary<string, string>
            {
                { DatabaseConnectors.MSSQL_PROVIDER, _mssqlConnectionString},
                { DatabaseConnectors.POSGRE_PROVIDER, _postgreConnectionString}
            };
        }

        async System.Threading.Tasks.Task InitializePostgreAsync()
        {
            await PostgreSqlContainer.StartAsync()
                .ConfigureAwait(false);

            var postgresOptions = DatabaseConnectors.GetPostgresConfiguration(PostgreSqlContainer.GetConnectionString());
            var postgresConfiguration = new ConnectionConfiguration(postgresOptions);

            var postgresDbFactory = new Integration.Data.Postgre.DataContextFactory();
            var posgresDbContext = postgresDbFactory.CreateDbContext(postgresConfiguration.ConnectionString);

            posgresDbContext.Database.Migrate();
            await posgresDbContext.DisposeAsync();
        }

        async System.Threading.Tasks.Task IAsyncLifetime.DisposeAsync()
        {
            _connector.Dispose();
            
            await PostgreSqlContainer.DisposeAsync()
                .ConfigureAwait(false);
        }
    }
}