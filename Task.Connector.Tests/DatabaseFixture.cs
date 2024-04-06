using Microsoft.EntityFrameworkCore;
using Task.Connector.Models;
using Task.Connector.Tests.Constants;
using Task.Integration.Data.DbCommon;
using Testcontainers.PostgreSql;


namespace Task.Connector.Tests
{
    public class DatabaseFixture : IAsyncLifetime
    {
        public readonly PostgreSqlContainer PostgreSqlContainer;

        public DatabaseFixture()
        {
            PostgreSqlContainer = new PostgreSqlBuilder()
                .WithImage("postgres:latest")
                .Build();
        }

        private void SetDefaultData(DbContextFactory factory, string provider)
        {
            new DataManager(factory, provider).PrepareDbForTest();
        }

        System.Threading.Tasks.Task IAsyncLifetime.InitializeAsync()
        {
            return InitializePostgreAsync();
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

            SetDefaultData(new DbContextFactory(postgresConfiguration.ConnectionString), DatabaseConnectors.POSGRE_PROVIDER);
        }

        async System.Threading.Tasks.Task IAsyncLifetime.DisposeAsync()
        {
            await PostgreSqlContainer.DisposeAsync()
                .ConfigureAwait(false);
        }
    }
}