using Microsoft.EntityFrameworkCore;
using Task.Connector.Models;
using Task.Integration.Data.Models.Models;

namespace Task.Connector
{
    public class AppDbContext :DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            string postgreConnectionString = "Host=localhost;Port=5432;Username=postgres;Password=241977;Database=testDb";
            if(!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseNpgsql(postgreConnectionString);
            }
        }

        public DbSet<ItRole> ItRoles { get; set; }

        public DbSet<UserPassword> Passwords { get; set; }

        public DbSet<RequestRight> RequestRights { get; set; }

        public DbSet<User> Users { get; set; }

        public DbSet<UserITRole> UserITRoles { get; set; }

        public DbSet<UserRequestRight> UserRequestRights { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<UserPassword>()
              .HasKey(password => password.Id);

            modelBuilder.Entity<RequestRight>()
                .HasKey(requestRight => requestRight.Id);

            modelBuilder.Entity<RequestRight>()
                .Property(r => r.Id)
                .ValueGeneratedOnAdd();

            modelBuilder.Entity<User>()
              .HasKey(user => user.Login);          

            modelBuilder.Entity<UserITRole>()
              .HasKey(userITRole => new
              {
                  userITRole.RoleId,
                  userITRole.UserId
              }
              );

            modelBuilder.Entity<UserRequestRight>()
              .HasKey(userRequestRight => new
              {
                  userRequestRight.UserId,
                  userRequestRight.RightId
              }
              );
        }       
    }
}
