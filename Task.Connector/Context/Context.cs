using Microsoft.EntityFrameworkCore;
using Task.Connector.Model;
using Task.Integration.Data.Models.Models;

namespace Task.Connector.Context
{
    internal class Context
    {
        public class DatabaseContext : DbContext
        {
            public DatabaseContext(DbContextOptions<DatabaseContext> options) : base(options) { }

            public DbSet<User> Users { get; set; }
            public DbSet<Password> Passwords { get; set; }
            public DbSet<RequestRight> RequestRights { get; set; }
            public DbSet<ItRole> ItRoles { get; set; }
            public DbSet<UserRequestRight> UserRequestRights { get; set; }
            public DbSet<UserItRole> UserItRoles { get; set; }

            protected override void OnModelCreating(ModelBuilder modelBuilder)
            {
                modelBuilder.Entity<UserRequestRight>()
                    .HasKey(ur => new { ur.UserId, ur.RequestRightId });

                modelBuilder.Entity<UserItRole>()
                    .HasKey(ui => new { ui.UserId, ui.ItRoleId });
            }
        }
    }
}
