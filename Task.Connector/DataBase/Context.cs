using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;
using Task.Integration.Data.Models;

namespace Task.Connector.DataBase
{
    /// <summary>
    /// Контекст подключаемой БД для хранения пользователей
    /// </summary>
    internal class Context:DbContext
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="connectionString">Строка настроек
        /// Настройки указываются в виде: настройка1='значение1';настройка2='значение2';...
        /// Настройки: Provider, ConnectionString, SchemaName</param>
        public Context(string connectionString):base()
        {

            Regex regex = new(@"(?<key>\w+)='(?<value>[^']+)';");
            var properties = regex.Matches(connectionString);

            _providers = properties.First(i => i.Groups["key"].Value == "Provider").Groups["value"].Value;
            _connectionString = properties.First(i=>i.Groups["key"].Value == "ConnectionString").Groups["value"].Value;
            _schema = properties.First(i => i.Groups["key"].Value == "SchemaName").Groups["value"].Value;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="connectionString">Строка настроек
        /// Настройки указываются в виде: настройка1='значение1';настройка2='значение2';...
        /// Настройки: Provider, ConnectionString, SchemaName</param>
        /// <param name="options">Внутренние опции БД</param>
        public Context(string connectionString, DbContextOptions<Context> options) : base(options)
        {
            this._connectionString = connectionString;
            Database.EnsureCreated();
        }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            //Исходя из настроек выбираем провайдера БД
            if (_providers.Contains("SqlServer"))
                optionsBuilder.UseSqlServer(_connectionString);
            else if (_providers.Contains("PostgreSQL"))
                optionsBuilder.UseNpgsql(_connectionString);
            else
                throw new ArgumentException("Database provider is not suppored");

            //Установка логгирования БД
            optionsBuilder.LogTo((id, l) => l == Microsoft.Extensions.Logging.LogLevel.Warning, (d) => logger?.Warn(d.ToString()));
            optionsBuilder.LogTo((id, l) => l == Microsoft.Extensions.Logging.LogLevel.Debug, (d) => logger?.Debug(d.ToString()));
            optionsBuilder.LogTo((id, l) => l == Microsoft.Extensions.Logging.LogLevel.Error, (d) => logger?.Error(d.ToString()));

            base.OnConfiguring(optionsBuilder);
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            //Схема таблиц
            builder.HasDefaultSchema(_schema);

            //Установка связи многие ко многим между User и RequestRight
            builder.Entity<User>()
                .HasMany(user => user.RequestRights)
                .WithMany(right => right.Users)
                .UsingEntity<UserRequestRight>(
                    i => i.HasOne(ur => ur.Right)
                        .WithMany(r => r.UserRight)
                        .HasForeignKey(ur => ur.RightId),
                    i =>i.HasOne(ur=>ur.User)
                        .WithMany(u=>u.UserRight)
                        .HasForeignKey(ur=>ur.UserId),
                    i => {
                        i.HasKey(ur => new { ur.UserId, ur.RightId });
                        i.ToTable("UserRequestRight");
                    }
                );

            //Установка связи многие ко многим между User и ITRole
            builder.Entity<User>()
                .HasMany(user => user.Roles)
                .WithMany(roles => roles.Users)
                .UsingEntity<UserITRole>(
                    i => i.HasOne(ur => ur.Role)
                        .WithMany(r => r.UserRoles)
                        .HasForeignKey(ur => ur.RoleId),
                    i => i.HasOne(ur => ur.User)
                        .WithMany(u => u.UserRoles)
                        .HasForeignKey(ur => ur.UserId),
                    i => {
                        i.HasKey(ur => new { ur.UserId, ur.RoleId });
                        i.ToTable("UserITRole");
                    }
                );

            //Установка связи один к одному между User и UserPassword
            builder.Entity<UserPassword>()
                .HasOne(pass => pass.User)
                .WithOne(user => user.Passwords)
                .HasForeignKey<UserPassword>(pass => pass.UserId);

            builder.Entity<User>().HasKey(i => i.Login);

            base.OnModelCreating(builder);
        }

        #region Items
        public DbSet<User> User { get; set; } = null!;
        public DbSet<UserPassword> Password { get; set; } = null!;
        public DbSet<ItRole> ItRole { get; set; } = null!;
        public DbSet<RequestRight> RequestRight { get; set; } = null!;

        #endregion //Items

        public ILogger? logger { get; set; }

        #region PrivateFileds
        private readonly string _connectionString;
        private readonly string _providers;
        private readonly string _schema;
        #endregion //PrivateFileds
    }
}
