using Microsoft.EntityFrameworkCore;
using Task.Integration.Data.DbCommon.DbModels;

namespace Task.Connector.Database
{
    public class DataBaseContext : DbContext
    {

        private readonly string _connectionString;

        public bool Connected => Database.CanConnect();

        public DbSet<User> Users { get; set; } = null!;
        public DbSet<ITRole> ITRoles { get; set; } = null!;
        public DbSet<UserITRole> UserITRoles { get; set; } = null!;
        public DbSet<Sequrity> Sequrities { get; set; } = null!;
        public DbSet<RequestRight> RequestRights { get; set; } = null!;
        public DbSet<UserRequestRight> UserRequestRights { get; set; } = null!;

        public DataBaseContext(string connectionString)
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
            modelBuilder.Entity<UserITRole>().HasKey(r => new { r.UserId, r.RoleId });
            modelBuilder.Entity<UserRequestRight>().HasKey(r => new { r.UserId, r.RightId });
        }
    }
}