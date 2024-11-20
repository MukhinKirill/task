namespace Task.DbModule.Models
{
	public class Password
	{
		public required uint Id { get; set; }
		public required string PasswordHash { get; set; }
		public required int UserId { get; set; }
		public User? User { get; set; }
	}
}