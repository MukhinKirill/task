using Microsoft.EntityFrameworkCore;
using Task.Connector.Models;
using Task.Connector.Tests.Constants;
using Task.Integration.Data.DbCommon;
using Testcontainers.MsSql;
using Testcontainers.PostgreSql;


namespace Task.Connector.Tests
{
    public class DatabaseFixture : IAsyncLifetime
    {
        public readonly PostgreSqlContainer PostgreSqlContainer;

        public readonly MsSqlContainer MsSqlContainer;

        public DatabaseFixture()
        {
            PostgreSqlContainer = new PostgreSqlBuilder()
                .Build();

            //MsSqlContainer = new MsSqlBuilder()
            //    .Build();
        }

        private void SetDefaultData(DbContextFactory factory, string provider)
        {
            new DataManager(factory, provider).PrepareDbForTest();
        }

        async System.Threading.Tasks.Task IAsyncLifetime.InitializeAsync()
        {
            await InitializePostgreAsync();

            await InitializeMssqlAsync();
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

        async System.Threading.Tasks.Task InitializeMssqlAsync()
        {
            await MsSqlContainer.StartAsync()
                .ConfigureAwait(false);

            var msSqlOptions = DatabaseConnectors.GetMssqlConfiguration(MsSqlContainer.GetConnectionString());
            var msSqlConfiguration = new ConnectionConfiguration(msSqlOptions);

            var msSqlDbFactory = new Integration.Data.MSSQL.DataContextFactory();
            var msSqlDbContext = msSqlDbFactory.CreateDbContext(msSqlConfiguration.ConnectionString);

            msSqlDbContext.Database.Migrate();
            await msSqlDbContext.DisposeAsync();

            SetDefaultData(new DbContextFactory(msSqlConfiguration.ConnectionString), DatabaseConnectors.MSSQL_PROVIDER);
        }

        async System.Threading.Tasks.Task IAsyncLifetime.DisposeAsync()
        {
            await PostgreSqlContainer.DisposeAsync()
                .ConfigureAwait(false);
        }
    }
}