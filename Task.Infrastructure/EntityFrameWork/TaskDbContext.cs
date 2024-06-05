using Microsoft.EntityFrameworkCore;
using Task.Domain.Roles;
using Task.Domain.Users;

namespace Task.Infrastructure.EntityFrameWork;

public class TaskDbContext : DbContext
{
    public TaskDbContext(DbContextOptions<TaskDbContext> options)
        : base(options)
    {
    }
    
    public virtual DbSet<User> Users { get; set; }
  //  public virtual DbSet<ItRole> ItRoles { get; set; }
  //  public virtual DbSet<RequestRight> RequestRights { get; set; }
  //  public virtual DbSet<Password> Passwords { get; set; }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("TestTaskSchema");
        
        modelBuilder.Entity<User>(u =>
        {
            u.HasKey(u => u.login);
            /*u.HasMany(u => u.itRoles)
                .WithMany(r => r.users);
            u.HasMany(u => u.requestRights)
                .WithMany(rr => rr.users);*/
        });
        
        /*modelBuilder.Entity<Password>(p =>
        {
            p.HasOne(p => p.userId);
        });*/
        
        base.OnModelCreating(modelBuilder);
    }
}