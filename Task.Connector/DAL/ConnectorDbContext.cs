using Microsoft.EntityFrameworkCore;
using Task.Integration.Data.DbCommon.DbModels;

namespace Task.Connector.DAL
{
    public class ConnectorDbContext : DbContext
    {
        private readonly string _connectionString;
        public ConnectorDbContext(string connectionString)
        {
            _connectionString = connectionString;
        }
        public DbSet<ITRole> ITRoles { get; set; }
        public DbSet<RequestRight> RequestRights { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<UserITRole> UsersITRoles { get; set; }
        public DbSet<UserRequestRight> UserRequestRights { get; set; }
        public DbSet<Sequrity> Securities { get; set; }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(_connectionString);
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<UserITRole>().HasKey(r => new { r.UserId, r.RoleId });
            modelBuilder.Entity<UserRequestRight>().HasKey(r => new { r.UserId, r.RightId });
        }
    }
}
