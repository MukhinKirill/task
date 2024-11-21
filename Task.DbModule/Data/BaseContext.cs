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
		public DbSet<ITRole> ITRoles { get; set; }
		public DbSet<UserITRole> UserITRoles { get; set; }
		public DbSet<RequestRight> RequestRights { get; set; }
		public DbSet<UserRequestRight> UserRequestRights { get; set; }

		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			modelBuilder.Entity<ITRole>(role =>
			{
				role.ToTable("ITRole", "TestTaskSchema");
				role.HasKey(r => r.Id);

				role.HasMany(r => r.UserITRoles).WithOne(ur => ur.ITRole)
					.HasForeignKey(ur => ur.ITRoleId);

				role.Property(r => r.Id).HasColumnName("id");
				role.Property(r => r.Name).HasColumnName("name");
				role.Property(r => r.CorporatePhoneNumber).HasColumnName("corporatePhoneNumber");
			});

			modelBuilder.Entity<RequestRight>(request =>
			{
				request.ToTable("RequestRight", "TestTaskSchema");

				request.HasKey(re => re.Id);

				request.HasMany(re => re.UserRequestRights).WithOne(ure => ure.RequestRight)
					.HasForeignKey(ure => ure.RequestRightId);

				request.Property(re => re.Id).HasColumnName("id");
				request.Property(re => re.Name).HasColumnName("name");
			});

			modelBuilder.Entity<User>(user =>
			{
				user.ToTable("User", "TestTaskSchema");

				user.HasKey(u => u.Login);
				user.HasMany(u => u.Passwords).WithOne(p => p.User)
					.HasForeignKey(p => p.UserLogin);

				user.HasMany(u => u.UserITRoles).WithOne(ur => ur.User)
					.HasForeignKey(ur => ur.UserLogin);

				user.HasMany(u => u.UserRequestRights).WithOne(ure => ure.User)
					.HasForeignKey(ure => ure.UserLogin);

				user.Property(u => u.Login).HasColumnName("login");
				user.Property(u => u.FirstName).HasColumnName("firstName");
				user.Property(u => u.LastName).HasColumnName("lastName");
				user.Property(u => u.MiddleName).HasColumnName("middleName");
				user.Property(u => u.TelephoneNumber).HasColumnName("telephoneNumber");
				user.Property(u => u.IsLead).HasColumnName("isLead");
			});

			modelBuilder.Entity<Password>(pass =>
			{
				pass.ToTable("Passwords", "TestTaskSchema");

				pass.HasKey(p => p.Id);

				pass.Property(p => p.Id).HasColumnName("id");
				pass.Property(p => p.UserLogin).HasColumnName("userId");
				pass.Property(p => p.PasswordHash).HasColumnName("password");
			});

			modelBuilder.Entity<UserITRole>(usRole =>
			{
				usRole.ToTable("UserITRole", "TestTaskSchema");

				usRole.HasKey(ur => new { ItRoleId = ur.ITRoleId, ur.UserLogin });

				usRole.Property(us => us.UserLogin).HasColumnName("userId");
				usRole.Property(us => us.ITRoleId).HasColumnName("roleId");
			});

			modelBuilder.Entity<UserRequestRight>(usRequest =>
			{
				usRequest.ToTable("UserRequestRight", "TestTaskSchema");

				usRequest.HasKey(ur => new { ur.UserLogin, ur.RequestRightId });

				usRequest.Property(usr => usr.UserLogin).HasColumnName("userId");
				usRequest.Property(usr => usr.RequestRightId).HasColumnName("rightId");
			});
		}
	}
}