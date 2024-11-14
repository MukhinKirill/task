using Microsoft.EntityFrameworkCore;
using Task.Integration.Data.DbCommon.DbModels;

namespace Task.Connector;

public class AppDbContext : DbContext
{
    public DbSet<User> Users { get; set; }
    public DbSet<Sequrity> Passwords { get; set; }
    public DbSet<ITRole> ItRoles { get; set; }
    public DbSet<RequestRight> RequestRights { get; set; }
    public DbSet<UserITRole> UserItRoles { get; set; }
    public DbSet<UserRequestRight> UserRequestRights { get; set; }

    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>()
            .ToTable("User", "TestTaskSchema")
            .HasKey(u => u.Login);
        modelBuilder.Entity<Sequrity>()
            .ToTable("Passwords", "TestTaskSchema")
            .HasKey(s => s.Id);
        modelBuilder.Entity<ITRole>()
            .ToTable("ItRole", "TestTaskSchema")
            .HasKey(r => r.Id);
        modelBuilder.Entity<RequestRight>()
            .ToTable("RequestRight", "TestTaskSchema")
            .HasKey(r => r.Id);
        modelBuilder.Entity<UserITRole>()
            .ToTable("UserITRole", "TestTaskSchema")
            .HasKey(uitr => new { uitr.UserId, uitr.RoleId });
        modelBuilder.Entity<UserRequestRight>()
            .ToTable("UserRequestRight", "TestTaskSchema")
            .HasKey(urr => new { urr.UserId, urr.RightId });
    }
}