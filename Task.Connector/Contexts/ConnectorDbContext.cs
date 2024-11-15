using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Task.Connector.Models;

namespace Task.Connector.Contexts;

public partial class ConnectorDbContext : DbContext
{
    public string connectionString;
    public ConnectorDbContext()
    {
    }

    public ConnectorDbContext(DbContextOptions<ConnectorDbContext> options)
        : base(options)
    {
    }

    public ConnectorDbContext(string connectionString)
    {
        this.connectionString = connectionString;  
    }

    public virtual DbSet<ItRole> ItRoles { get; set; }

    public virtual DbSet<MigrationHistory> MigrationHistories { get; set; }

    public virtual DbSet<Password> Passwords { get; set; }

    public virtual DbSet<RequestRight> RequestRights { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<UserItrole> UserItroles { get; set; }

    public virtual DbSet<UserRequestRight> UserRequestRights { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        var config = new ConfigurationBuilder()
                        .AddJsonFile("appsettings.json")
                        .SetBasePath(Directory.GetCurrentDirectory())
                        .Build();

        optionsBuilder.UseNpgsql(config.GetConnectionString("DefaultConnection"));
    }
       

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {

        modelBuilder.Entity<ItRole>(entity =>
        {
            entity.ToTable("ItRole", "TestTaskSchema");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CorporatePhoneNumber)
                .HasMaxLength(4)
                .HasColumnName("corporatePhoneNumber");
            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .HasColumnName("name");
        });

        modelBuilder.Entity<MigrationHistory>(entity =>
        {
            entity.HasKey(e => e.MigrationId);

            entity.ToTable("_MigrationHistory", "TestTaskSchema");

            entity.Property(e => e.MigrationId).HasMaxLength(150);
            entity.Property(e => e.ProductVersion).HasMaxLength(32);
        });

        modelBuilder.Entity<Password>(entity =>
        {
            entity.ToTable("Passwords", "TestTaskSchema");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Password1)
                .HasMaxLength(20)
                .HasColumnName("password");
            entity.Property(e => e.UserId)
                .HasMaxLength(22)
                .HasColumnName("userId");
        });

        modelBuilder.Entity<RequestRight>(entity =>
        {
            entity.ToTable("RequestRight", "TestTaskSchema");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Name).HasColumnName("name");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Login);

            entity.ToTable("User", "TestTaskSchema");

            entity.Property(e => e.Login)
                .HasMaxLength(22)
                .HasColumnName("login");
            entity.Property(e => e.FirstName)
                .HasMaxLength(20)
                .HasColumnName("firstName");
            entity.Property(e => e.IsLead).HasColumnName("isLead");
            entity.Property(e => e.LastName)
                .HasMaxLength(20)
                .HasColumnName("lastName");
            entity.Property(e => e.MiddleName)
                .HasMaxLength(20)
                .HasColumnName("middleName");
            entity.Property(e => e.TelephoneNumber)
                .HasMaxLength(20)
                .HasColumnName("telephoneNumber");
        });

        modelBuilder.Entity<UserItrole>(entity =>
        {
            entity.HasKey(e => new { e.RoleId, e.UserId });

            entity.ToTable("UserITRole", "TestTaskSchema");

            entity.Property(e => e.RoleId).HasColumnName("roleId");
            entity.Property(e => e.UserId)
                .HasMaxLength(22)
                .HasColumnName("userId");
        });

        modelBuilder.Entity<UserRequestRight>(entity =>
        {
            entity.HasKey(e => new { e.RightId, e.UserId });

            entity.ToTable("UserRequestRight", "TestTaskSchema");

            entity.Property(e => e.RightId).HasColumnName("rightId");
            entity.Property(e => e.UserId)
                .HasMaxLength(22)
                .HasColumnName("userId");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    private void OnModelCreatingPartial(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>()
            .HasMany(u => u.Passwords) 
            .WithOne(p => p.User) 
            .HasForeignKey(p => p.UserId) 
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<UserItrole>()
            .HasOne(uit => uit.User)
            .WithMany(u => u.UserItroles) 
            .HasForeignKey(uit => uit.UserId);

        modelBuilder.Entity<UserItrole>()
            .HasOne(uit => uit.ItRole)
            .WithMany(ir => ir.UserItroles) 
            .HasForeignKey(uit => uit.RoleId);

        modelBuilder.Entity<UserRequestRight>()
            .HasOne(urr => urr.User)
            .WithMany(u => u.UserRequestRights) 
            .HasForeignKey(urr => urr.UserId);

        modelBuilder.Entity<UserRequestRight>()
            .HasOne(urr => urr.RequestRight)
            .WithMany(rr => rr.UserRequestRights) 
            .HasForeignKey(urr => urr.RightId);
    }
}
