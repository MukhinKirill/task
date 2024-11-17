using Microsoft.EntityFrameworkCore;

using Task.Integration.Data.DbCommon.DbModels;

namespace Task.Connector
{
    public class ApplicationContext : DbContext
    {
        public static string? ConnString { get; set; }  = null;
        public DbSet<User> Users => Set<User>();
        public DbSet<Sequrity> Passwords => Set<Sequrity>();
        public DbSet<RequestRight> RequestRights => Set<RequestRight>();
        public DbSet<ITRole> ITRoles => Set<ITRole>();
        public DbSet<UserITRole> UserITRoles => Set<UserITRole>();
        public DbSet<UserRequestRight> UserRequestRights => Set<UserRequestRight>();

        
        public ApplicationContext(string connectionString)
        {
            ConnString = connectionString;


        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                if (string.IsNullOrEmpty(ConnString))
                {
                    throw new ArgumentException("Connection string is null or empty", nameof(ConnString));
                }

                optionsBuilder.UseNpgsql(ConnString);
            }

        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<UserITRole>()
                .HasKey(ur => new { ur.UserId, ur.RoleId });

            modelBuilder.Entity<UserRequestRight>()
                .HasKey(ur => new { ur.UserId, ur.RightId });
        }
    }
}
