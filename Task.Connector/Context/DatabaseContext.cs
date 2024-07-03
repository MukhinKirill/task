using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Task.Connector.Entities;

namespace Task.Connector.Context;

public partial class DatabaseContext : DbContext
{
    public DatabaseContext(DbContextOptions<DatabaseContext> options)
        : base(options)
    {
    }

    public virtual DbSet<ItRole> ItRoles { get; set; }

    public virtual DbSet<Password> Passwords { get; set; }

    public virtual DbSet<RequestRight> RequestRights { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<UserITRole> UserITRoles { get; set; }

    public virtual DbSet<UserRequestRight> UserRequestRights { get; set; }

    public virtual DbSet<_MigrationHistory> _MigrationHistories { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        => optionsBuilder.UseNpgsql("Server=127.0.0.1;Port=5432;Database=avanpostDb;Username=postgres;Password=123;"); //"Server=127.0.0.1;Port=5432;Database=avanpostDb;Username=postgres;Password=123;"

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ItRole>(entity =>
        {
            entity.ToTable("ItRole", "TestTaskSchema");

            entity.Property(e => e.corporatePhoneNumber).HasMaxLength(4);
            entity.Property(e => e.name).HasMaxLength(100);
        });

        modelBuilder.Entity<Password>(entity =>
        {
            entity.ToTable("Passwords", "TestTaskSchema");

            entity.Property(e => e.password1)
                .HasMaxLength(20)
                .HasColumnName("password");
            entity.Property(e => e.userId).HasMaxLength(22);
        });

        modelBuilder.Entity<RequestRight>(entity =>
        {
            entity.ToTable("RequestRight", "TestTaskSchema");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.login);

            entity.ToTable("User", "TestTaskSchema");

            entity.Property(e => e.login).HasMaxLength(22);
            entity.Property(e => e.firstName).HasMaxLength(20);
            entity.Property(e => e.lastName).HasMaxLength(20);
            entity.Property(e => e.middleName).HasMaxLength(20);
            entity.Property(e => e.telephoneNumber).HasMaxLength(20);
        });

        modelBuilder.Entity<UserITRole>(entity =>
        {
            entity.HasKey(e => new { e.roleId, e.userId });

            entity.ToTable("UserITRole", "TestTaskSchema");

            entity.Property(e => e.userId).HasMaxLength(22);
        });

        modelBuilder.Entity<UserRequestRight>(entity =>
        {
            entity.HasKey(e => new { e.rightId, e.userId });

            entity.ToTable("UserRequestRight", "TestTaskSchema");

            entity.Property(e => e.userId).HasMaxLength(22);
        });

        modelBuilder.Entity<_MigrationHistory>(entity =>
        {
            entity.HasKey(e => e.MigrationId);

            entity.ToTable("_MigrationHistory", "TestTaskSchema");

            entity.Property(e => e.MigrationId).HasMaxLength(150);
            entity.Property(e => e.ProductVersion).HasMaxLength(32);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
