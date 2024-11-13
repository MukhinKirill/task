using Microsoft.EntityFrameworkCore;
using Task.Connector.Models;

namespace Task.Connector
{
    public class ApplicationContext : DbContext
    {

        public DbSet<User> User => Set<User>();
        public DbSet<ItRole> ItRole => Set<ItRole>();
        public DbSet<Passwords> Passwords => Set<Passwords>();
        public DbSet<RequestRight> RequestRights => Set<RequestRight>();
        public DbSet<UserITRole> UsersItRole => Set<UserITRole>();
        public DbSet<UserRequestRight> UsersRequestRight => Set<UserRequestRight>();
        public DbSet<MigrationHistory> _MigrationHistory => Set<MigrationHistory>();

        private readonly string _connectionString;

        public ApplicationContext(string connectionString)
        {
            _connectionString = connectionString;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseNpgsql(_connectionString);

            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>().ToTable(nameof(User), "TestTaskSchema");
            modelBuilder.Entity<ItRole>().ToTable(nameof(ItRole), "TestTaskSchema");
            modelBuilder.Entity<Passwords>().ToTable(nameof(Passwords), "TestTaskSchema");
            modelBuilder.Entity<RequestRight>().ToTable(nameof(RequestRight), "TestTaskSchema");
            modelBuilder.Entity<UserITRole>().ToTable(nameof(UserITRole), "TestTaskSchema");
            modelBuilder.Entity<UserRequestRight>().ToTable(nameof(UserRequestRight), "TestTaskSchema");
            modelBuilder.Entity<MigrationHistory>().ToTable("_MigrationHistory", "TestTaskSchema");

            modelBuilder.Entity<UserITRole>().HasKey(userItRole => new { userItRole.RoleId, userItRole.UserId });
            modelBuilder.Entity<UserRequestRight>().HasKey(userRequestRight => new { userRequestRight.RightId, userRequestRight.UserId });

            base.OnModelCreating(modelBuilder);
        }


    }
}
