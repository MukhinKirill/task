using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using Task.Connector.DB.Models;
using Task.Integration.Data.Models;

namespace Task.Connector.DB
{
    // �������� ���� ������ ��� ����������, ���������� �������� � ����������� �����������
    internal class ApplicationContext : DbContext
    {
        private readonly string connectionString;
        private readonly string provider;

        // ����������� ������
        public DbSet<ItRole> ItRoles { get; set; }
        public DbSet<Password> Passwords { get; set; }
        public DbSet<RequestRight> RequestRights { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<UserITRole> UserITRoles { get; set; }
        public DbSet<UserRequestRight> UserRequestRights { get; set; }
        public ApplicationContext(string connectionString)
        {
            // ���������� ���������� ��������� ��� ������ ���������� � ������ �����������
            var providerMatch = Regex.Match(connectionString, "Provider='.*?';");

            // ���������, ������ �� ���������, ���� ��� � ������� ����������
            if (!providerMatch.Success)
            {
                throw new ArgumentException("�� ������ ��������� � ������ �����������.");
            }

            // ���������� ��� ���������� �� ��������, ������������� � ������ �����������
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
                throw new ArgumentException("���������������� ���������.");
            }

            // ���������� ���������� ��������� ��� ������ ������ �����������
            var connectionMatch = Regex.Match(connectionString, "ConnectionString='.*?';");

            // ���� ������ ����������� �������, ��������� �� ��������
            if (connectionMatch.Success)
            {
                this.connectionString = connectionMatch.Value.Split("'")[1];
            }
            else
            {
                // ���� ������ ����������� �� �������, ����������� ����������
                throw new ArgumentException("������������ ������ �����������.");
            }
        }



        // ������������� ��������� ���� ������ � ����������� �� ����
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

        // ��������� ������� � ��������� ������
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
