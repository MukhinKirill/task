using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using Task.Connector.DB.Models;
using Task.Integration.Data.Models;

namespace Task.Connector.DB
{
    // Контекст базы данных для приложения, определяет сущности и настраивает подключение
    internal class ApplicationContext : DbContext
    {
        private readonly string connectionString;
        private readonly string provider;

        // Определение таблиц
        public DbSet<ItRole> ItRoles { get; set; }
        public DbSet<Password> Passwords { get; set; }
        public DbSet<RequestRight> RequestRights { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<UserITRole> UserITRoles { get; set; }
        public DbSet<UserRequestRight> UserRequestRights { get; set; }
        public ApplicationContext(string connectionString)
        {
            // Используем регулярное выражение для поиска провайдера в строке подключения
            var providerMatch = Regex.Match(connectionString, "Provider='.*?';");

            // Проверяем, найден ли провайдер, если нет — бросаем исключение
            if (!providerMatch.Success)
            {
                throw new ArgumentException("Не указан провайдер в строке подключения.");
            }

            // Определяем тип провайдера по значению, содержащемуся в строке подключения
            if (providerMatch.Value.Contains("SqlServer"))
            {
                provider = "MSSQL";
            }
            else if (providerMatch.Value.Contains("PostgreSQL"))
            {
                provider = "POSTGRE";
            }
            else
            {
                throw new ArgumentException("Неподдерживаемый провайдер.");
            }

            // Используем регулярное выражение для поиска строки подключения
            var connectionMatch = Regex.Match(connectionString, "ConnectionString='.*?';");

            // Если строка подключения найдена, извлекаем ее значение
            if (connectionMatch.Success)
            {
                this.connectionString = connectionMatch.Value.Split("'")[1];
            }
            else
            {
                // Если строка подключения не найдена, выбрасываем исключение
                throw new ArgumentException("Некорректная строка подключения.");
            }
        }



        // Конфигурируем провайдер базы данных в зависимости от типа
        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
            if (provider == "POSTGRE")
            {
                options.UseNpgsql(connectionString);
            }
            else if (provider == "MSSQL")
            {
                options.UseSqlServer(connectionString);
            }

        }

        // Настройка моделей и первичных ключей
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ItRole>().HasKey(itRole => itRole.Id);
            modelBuilder.Entity<Password>().HasKey(password => password.Id);
            modelBuilder.Entity<RequestRight>().HasKey(requestRight => requestRight.Id);
            modelBuilder.Entity<User>().HasKey(user => user.Login);
            modelBuilder.Entity<UserITRole>().HasKey(userITRole => new { userITRole.RoleId, userITRole.UserId });
            modelBuilder.Entity<UserRequestRight>().HasKey(userRequestRight => new { userRequestRight.UserId, userRequestRight.RightId });
        }
    }
}
