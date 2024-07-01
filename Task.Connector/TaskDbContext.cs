using Microsoft.EntityFrameworkCore;
using Task.Connector.Entities;
using Task.Connector.Entities.Configuration;

public class TaskDbContext : DbContext
{
    public DbSet<ItRole> ItRoles => Set<ItRole>();
    public DbSet<Password> Passwords => Set<Password>();
    public DbSet<RequestRight> RequestRights => Set<RequestRight>();
    public DbSet<User> Users => Set<User>();
    public DbSet<UserItRole> UserItroles => Set<UserItRole>();
    public DbSet<UserRequestRight> UserRequestRights => Set<UserRequestRight>();

    public TaskDbContext(DbContextOptions options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new ItRoleConfiguration());
        modelBuilder.ApplyConfiguration(new PasswordConfiguration());
        modelBuilder.ApplyConfiguration(new RequestRightConfiguration());
        modelBuilder.ApplyConfiguration(new UserConfiguration());
        modelBuilder.ApplyConfiguration(new UserItRoleConfiguration());
        modelBuilder.ApplyConfiguration(new UserRequestRightConfiguration());
    }
}
