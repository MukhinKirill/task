namespace Task.DbModule.Models
{
	public class Password
	{
		public int Id { get; set; }
		public required string UserLogin { get; set; }
		public required string PasswordHash { get; set; }
		public User? User { get; set; }
	}
}