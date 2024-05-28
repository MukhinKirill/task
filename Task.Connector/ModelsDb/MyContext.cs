using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace Task.Connector.ModelsDb;

public partial class MyContext : DbContext
{
    public MyContext()
    {
    }

    public MyContext(DbContextOptions<MyContext> options, string schema)
        : base(options)
    {
        Schema = schema;
    }

    private readonly string _connectionString;
    public string Schema { get; }
    public virtual DbSet<Container> Containers { get; set; }

    public virtual DbSet<ItRole> ItRoles { get; set; }

    public virtual DbSet<MigrationHistory> MigrationHistories { get; set; }

    public virtual DbSet<Order> Orders { get; set; }

    public virtual DbSet<UserPassword> Passwords { get; set; }

    public virtual DbSet<RequestRight> RequestRights { get; set; }

    public virtual DbSet<Research> Researches { get; set; }

    public virtual DbSet<Rule> Rules { get; set; }

    public virtual DbSet<Sample> Samples { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<UserItrole> UserItroles { get; set; }

    public virtual DbSet<UserRequestRight> UserRequestRights { get; set; }

    //protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    //    => optionsBuilder.UseNpgsql("Server=127.0.0.1;Port=5435;Database=postgres;Username=postgres;Password=postgres;");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(Schema);

        modelBuilder.Entity<Container>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("containers_pk");

            entity.ToTable("containers");

            entity.Property(e => e.Id)
                .UseIdentityAlwaysColumn()
                .HasColumnName("id");
            entity.Property(e => e.Name)
                .HasColumnType("character varying")
                .HasColumnName("name");
        });

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

        modelBuilder.Entity<Order>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("orders_pk");

            entity.ToTable("orders");

            entity.Property(e => e.Id)
                .UseIdentityAlwaysColumn()
                .HasColumnName("id");
            entity.Property(e => e.CreatedDate)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_date");
            entity.Property(e => e.Fio)
                .HasColumnType("character varying")
                .HasColumnName("fio");
        });

        modelBuilder.Entity<UserPassword>(entity =>
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

        modelBuilder.Entity<Research>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("researches_pk");

            entity.ToTable("researches");

            entity.Property(e => e.Id)
                .UseIdentityAlwaysColumn()
                .HasColumnName("id");
            entity.Property(e => e.Name)
                .HasColumnType("character varying")
                .HasColumnName("name");
            entity.Property(e => e.Price)
                .HasColumnType("money")
                .HasColumnName("price");
        });

        modelBuilder.Entity<Rule>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("rules_pk");

            entity.ToTable("rules");

            entity.Property(e => e.Id)
                .UseIdentityAlwaysColumn()
                .HasColumnName("id").ValueGeneratedNever();
            entity.Property(e => e.ContainerId).HasColumnName("container_id");
            entity.Property(e => e.ResearcheId)
                .ValueGeneratedOnAdd()
                .HasColumnName("researche_id");
        });

        modelBuilder.Entity<Sample>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("samples_pk");

            entity.ToTable("samples");

            entity.Property(e => e.Id)
                .UseIdentityAlwaysColumn()
                .HasColumnName("id");
            entity.Property(e => e.ContainerId).HasColumnName("container_id");
            entity.Property(e => e.Number).HasColumnName("number");
            entity.Property(e => e.OrderId).HasColumnName("order_id");
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
