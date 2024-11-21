namespace Task.DbModule.Models
{
	public class User
	{
		public required string Login { get; set; }
		public required string LastName { get; set; }
		public required string FirstName { get; set; }
		public required string MiddleName { get; set; }
		public required string TelephoneNumber { get; set; }
		public required bool IsLead { get; set; }
		public ICollection<Password>? Passwords { get; set; }
		public ICollection<UserITRole>? UserITRoles { get; set; }
		public ICollection<UserRequestRight>? UserRequestRights { get; set; }
	}
}