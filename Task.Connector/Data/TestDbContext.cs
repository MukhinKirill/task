using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;

namespace Task.Connector.Data;

public partial class TestDbContext : DbContext
{
    private string _connectionString;
    public TestDbContext(string connString)
    {
        var connectionMatch = Regex.Match(connString, "ConnectionString='.*?;'");

        if (connectionMatch.Success)
        {
            _connectionString = connectionMatch.Value.Split("'").Skip(1).First();
        }
        else
        {
            throw new ArgumentException();
        }
    }

    public virtual DbSet<ItRole> ItRoles { get; set; }

    public virtual DbSet<PasswordEntity> Passwords { get; set; }

    public virtual DbSet<RequestRight> RequestRights { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<UserItrole> UserItroles { get; set; }

    public virtual DbSet<UserRequestRight> UserRequestRights { get; set; }


    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            optionsBuilder.UseSqlServer(_connectionString);
        }
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

        modelBuilder.Entity<PasswordEntity>(entity =>
        {
            entity.ToTable("Passwords", "TestTaskSchema");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Password)
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

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
