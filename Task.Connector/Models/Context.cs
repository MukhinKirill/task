using Microsoft.EntityFrameworkCore;

namespace Task.Connector.Models;

public class Context: DbContext
{
    public Context(DbContextOptions<Context> options) : base(options) {}
    
    public DbSet<User> Users { get; set; }
    public DbSet<UserItRole> UsersItRoles { get; set; }
    public DbSet<UserRequestRight> UserRequestRights { get; set; }
    public DbSet<Password> Passwords { get; set; }
    public DbSet<RequestRight> RequestRights { get; set; }
    public DbSet<ItRole> ItRoles { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("TestTaskSchema");
        modelBuilder.Entity<ItRole>(e =>
        {
            e.ToTable("ItRole");
            e.HasKey(e => e.Id).HasName("PK_ItRole");
        });
        modelBuilder.Entity<Password>(e =>
        {
            e.ToTable("Passwords");
            e.HasKey(e => e.Id).HasName("PK_Passwords");
        });
        modelBuilder.Entity<RequestRight>(e =>
        {
            e.ToTable("RequestRight");
            e.HasKey(e => e.Id).HasName("PK_RequestRight");
        });
        modelBuilder.Entity<User>(e =>
        {
            e.ToTable("User");
            e.Property(e => e.Login).HasColumnName("login");
            e.HasKey(e => e.Login).HasName("PK_User");
        });
        modelBuilder.Entity<UserRequestRight>(e =>
        {
            e.ToTable("UserRequestRight");
            e.HasKey(e => new
            {
                e.UserId,
                e.RequestRightId
            });
        });

        modelBuilder.Entity<UserItRole>(e =>
        {
            e.ToTable("UserITRole");
            e.HasKey(e => new
            {
                e.UserId,
                e.ItRoleId
            });
        });
        
    }
    
}