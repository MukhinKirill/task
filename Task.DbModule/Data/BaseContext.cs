using Microsoft.EntityFrameworkCore;
using Task.DbModule.Models;

namespace Task.DbModule.Data
{
	public class BaseContext : Microsoft.EntityFrameworkCore.DbContext
	{
		public BaseContext(DbContextOptions<BaseContext> options) : base(options)
		{
		}

		public DbSet<User> Users { get; set; }
		public DbSet<Password> Passwords { get; set; }
		public DbSet<ItRole> ItRoles { get; set; }
		public DbSet<UserItRole> UserItRoles { get; set; }
		public DbSet<RequestRight> RequestRights { get; set; }
		public DbSet<UserRequestRight> UserRequestRights { get; set; }

		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			base.OnModelCreating(modelBuilder);
		}
	}
}