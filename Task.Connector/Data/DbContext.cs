using Microsoft.EntityFrameworkCore;
using Task.Connector.Data.Configurations;
using Task.Connector.Data.Entities;

namespace Task.Connector.Data;

public class TaskDbContext : DbContext
{
    private readonly string _scheme;
    public DbSet<ItRole> ItRole { get; set; }
    public DbSet<Password> Passwords { get; set; }
    public DbSet<RequestRight> RequestRight { get; set; }
    public DbSet<User> User { get; set; }
    public DbSet<UserItRole> UserItRole { get; set; }
    public DbSet<UserRequestRight> UserRequestRight { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(_scheme);
        
        modelBuilder.ApplyConfiguration(new ItRoleConfiguration());
        modelBuilder.ApplyConfiguration(new PasswordConfiguration());
        modelBuilder.ApplyConfiguration(new RequestRightConfiguration());
        modelBuilder.ApplyConfiguration(new UserConfiguration());
        modelBuilder.ApplyConfiguration(new UserItRoleConfiguration());
        modelBuilder.ApplyConfiguration(new UserRequestRightConfiguration());

        base.OnModelCreating(modelBuilder);
    }
    
    public TaskDbContext(DbContextOptions<TaskDbContext> options, string scheme)
        : base(options)
    {
        _scheme = scheme;
    }
}