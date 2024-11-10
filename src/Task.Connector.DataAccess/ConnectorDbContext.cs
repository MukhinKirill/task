using Microsoft.EntityFrameworkCore;
using Task.Connector.DataAccess.Configurations;
using Task.Connector.Domain;

namespace Task.Connector.DataAccess;

public class ConnectorDbContext : DbContext
{
    public DbSet<ItRole> ItRoles { get; set; }
    public DbSet<Security> Passwords { get; set; }
    public DbSet<RequestRight> RequestRights { get; set; }
    public DbSet<User> Users { get; set; }
    public DbSet<UserItRole> UsersItRoles { get; set; }
    public DbSet<UserRequestRight> UserRequestRights { get; set; }

    public ConnectorDbContext(DbContextOptions options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new SecurityConfiguration());

        base.OnModelCreating(modelBuilder);
    }
}
