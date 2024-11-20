namespace Task.DbModule.Models
{
	public class User
	{
		public required uint Id { get; set; }
		public required string Login { get; set; }
		public required int PasswordId { get; set; }
		public Password? Password { get; set; }
		public ICollection<UserItRole> UserItRoles { get; set; } = [];
		public ICollection<UserRequestRight> UserRequestRights { get; set; } = [];
	}
}