using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using Task.Connector.DataModels;

namespace Task.Connector;

internal class AppDatabaseContext : DbContext
{
  private readonly string _connectionString;
  private readonly string _provider;
    
  public AppDatabaseContext(string connectionString)
  {
    var providerMatch = Regex.Match(connectionString, "Provider='.*?';");

    if (!providerMatch.Success)
    {
      throw new ArgumentException();
    }

    if (providerMatch.Value.Contains("SqlServer"))
    {
      _provider = "MSSQL";
    }
    else if(providerMatch.Value.Contains("PostgreSQL"))
    {
      _provider = "POSTGRE";
    }
    else
    {
      throw new ArgumentException();
    }
    
    var connectionMatch = Regex.Match(connectionString, "ConnectionString='.*?;'");

    if (connectionMatch.Success)
    {
      _connectionString = connectionMatch.Value.Split("'").Skip(1).First();
    }
    else
    {
      throw new ArgumentException();
    }
  }

  public DbSet<ItRoleDataModel> ItRoles { get; set; }

  public DbSet<PasswordDataModel> Passwords { get; set; }

  public DbSet<RequestRightDataModel> RequestRights { get; set; }

  public DbSet<UserDataModel> Users { get; set; }

  public DbSet<UserITRoleDataModel> UserITRoles { get; set; }

  public DbSet<UserRequestRightDataModel> UserRequestRights { get; set; }

  protected override void OnConfiguring(DbContextOptionsBuilder options)
  {
    if (_provider == "POSTGRE")
    {
      options.UseNpgsql(_connectionString);
    }
    else if (_provider == "MSSQL")
    {
      options.UseSqlServer(_connectionString);
    }
  }

  protected override void OnModelCreating(ModelBuilder modelBuilder)
  {
    modelBuilder.Entity<ItRoleDataModel>()
      .HasKey(itRole => itRole.Id);

    modelBuilder.Entity<PasswordDataModel>()
      .HasKey(password => password.Id);

    modelBuilder.Entity<RequestRightDataModel>()
      .HasKey(requestRight => requestRight.Id);

    modelBuilder.Entity<UserDataModel>()
      .HasKey(user => user.Login);

    modelBuilder.Entity<UserITRoleDataModel>()
      .HasKey(userITRole => new
        {
          userITRole.RoleId,
          userITRole.UserId
        }
      );

    modelBuilder.Entity<UserRequestRightDataModel>()
      .HasKey(userRequestRight => new
        {
          userRequestRight.UserId,
          userRequestRight.RightId
        }
      );
  }
}