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
			modelBuilder.Entity<User>(user =>
			{
				user.HasKey(u => u.Login);
				user.HasOne(u => u.Password).WithOne(p => p.User)
					.HasForeignKey<Password>(p => p.UserLogin);

				user.HasMany(u => u.UserItRoles).WithOne(ur => ur.User)
					.HasForeignKey(ur => ur.UserLogin);

				user.HasMany(u => u.UserRequestRights).WithOne(ure => ure.User)
					.HasForeignKey(ure => ure.UserLogin);
			});

			modelBuilder.Entity<Password>(pass =>
			{
				pass.HasKey(p => p.UserLogin);
			});

			modelBuilder.Entity<ItRole>(role =>
			{
				role.HasKey(r => r.Id);

				role.HasMany(r => r.UserItRoles).WithOne(ur => ur.ItRole)
					.HasForeignKey(ur => ur.ItRoleId);
			});

			modelBuilder.Entity<UserItRole>(usRole =>
			{
				usRole.HasKey(ur => new { ur.ItRoleId, ur.UserLogin });
			});

			modelBuilder.Entity<RequestRight>(request =>
			{
				request.HasKey(re => re.Id);

				request.HasMany(re => re.UserRequestRights).WithOne(ure => ure.RequestRight)
					   .HasForeignKey(ure => ure.RequestRightId);
			});

			modelBuilder.Entity<UserRequestRight>(usRequest =>
			{
				usRequest.HasKey(ur => new { ur.UserLogin, ur.RequestRightId });
			});
		}
	}
}