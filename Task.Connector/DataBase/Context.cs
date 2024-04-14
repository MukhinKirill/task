using Microsoft.EntityFrameworkCore;
using Task.Integration.Data.Models;

namespace Task.Connector.DataBase
{
    internal class Context:DbContext
    {
        public Context(string connectionString):base()
        {
            this.connectionString = connectionString;
        }
        public Context(string connectionString, DbContextOptions<Context> options) : base(options)
        {
            this.connectionString = connectionString;
            Database.EnsureCreated();
        }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(connectionString);

            //Установка логгирования БД
            optionsBuilder.LogTo((id, l) => l == Microsoft.Extensions.Logging.LogLevel.Warning, (d) => logger?.Warn(d.ToString()));
            optionsBuilder.LogTo((id, l) => l == Microsoft.Extensions.Logging.LogLevel.Debug, (d) => logger?.Debug(d.ToString()));
            optionsBuilder.LogTo((id, l) => l == Microsoft.Extensions.Logging.LogLevel.Error, (d) => logger?.Error(d.ToString()));

            base.OnConfiguring(optionsBuilder);
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<User>()
                .HasMany(user => user.RequestRights)
                .WithMany(right => right.Users)
                .UsingEntity(j => j.ToTable("UserRequestRight"));

            builder.Entity<User>()
                .HasMany(user => user.Roles)
                .WithMany(roles => roles.Users)
                .UsingEntity(j => j.ToTable("UserITRole"));

            base.OnModelCreating(builder);
        }

        #region Items
        public DbSet<User> Users { get; set; } = null!;
        public DbSet<UserPassword> Passwords { get; set; } = null!;
        public DbSet<ItRole> ItRoles { get; set; } = null!;
        public DbSet<RequestRight> RequestRights { get; set; } = null!;

        #endregion

        public ILogger? logger { get; set; }
        private string connectionString;
    }
}
