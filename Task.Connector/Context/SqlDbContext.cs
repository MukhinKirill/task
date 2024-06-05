using Microsoft.EntityFrameworkCore;
using Task.Connector.Models;

namespace Task.Connector.Context
{
    public abstract class SqlDbContext : DbContext
    {
        protected SqlDbContext()
        {
        }
        protected SqlDbContext(DbContextOptions options) : base(options)
        {
        }

        public virtual DbSet<ItRole> ItRoles { get; set; }

        public virtual DbSet<MigrationHistory> MigrationHistories { get; set; }

        public virtual DbSet<Password> Passwords { get; set; }

        public virtual DbSet<RequestRight> RequestRights { get; set; }

        public virtual DbSet<User> Users { get; set; }

        public virtual DbSet<UserItrole> UserItroles { get; set; }

        public virtual DbSet<UserRequestRight> UserRequestRights { get; set; }
    }
}
