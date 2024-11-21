namespace Task.DbModule.Models
{
	public class UserITRole
	{
		public required string UserLogin { get; set; }
		public required int ITRoleId { get; set; }
		public User? User { get; set; }
		public ITRole? ITRole { get; set; }
	}
}