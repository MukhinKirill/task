namespace Task.DbModule.Models
{
	public class UserItRole
	{
		public required string UserLogin { get; set; }
		public required uint ItRoleId { get; set; }
		public User? User { get; set; }
		public ItRole? ItRole { get; set; }
	}
}