using Microsoft.EntityFrameworkCore;
using Models;
namespace Data
{
    public class AppDbContext : DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<Password> Passwords { get; set; }
        public DbSet<ItRole> ItRoles { get; set; }
        public DbSet<RequestRight> RequestRights { get; set; }
        public DbSet<UserITRole> UserITRoles { get; set; }
        public DbSet<UserRequestRight> UserRequestRights { get; set; }
        public DbSet<_MigrationHistory> _MigrationHistory { get; set; }

        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<_MigrationHistory>().HasNoKey();
            modelBuilder.Entity<User>()
        .HasKey(u => u.Login); 
            modelBuilder.Entity<UserITRole>()
                .HasKey(u => new { u.UserId, u.RoleId });

            modelBuilder.Entity<UserRequestRight>()
                .HasKey(u => new { u.UserId, u.RightId });
        }
    }

}
