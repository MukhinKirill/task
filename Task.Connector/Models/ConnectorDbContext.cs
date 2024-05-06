using Microsoft.EntityFrameworkCore;

namespace Task.Connector.Models
{
    public class ConnectorDbContext : DbContext
    {

        public DbSet<User> Users { get; set; }
		public DbSet<Password> Passwords { get; set; }

		public DbSet<UserRequestRight> UserRequestRights { get; set; }
		public DbSet<UserITRole> UserITRoles { get; set; }
		public DbSet<RequestRight> RequestRights { get; set; }
		public DbSet<ItRole> ItRoles { get; set; }

 
        private string _connectionString;
		private string _defaultSchema;

		public ConnectorDbContext(string connectionString,string defaultSchema)
        {
            _connectionString = connectionString;
			_defaultSchema = defaultSchema;
            //OnConfiguring(new DbContextOptionsBuilder());
            Database.EnsureCreated();
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseNpgsql(_connectionString);
        }

		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			modelBuilder.HasDefaultSchema(_defaultSchema);
		}
	}
}
