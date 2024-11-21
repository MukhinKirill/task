namespace Task.DbModule.Models
{
	public class UserRequestRight
	{
		public required string UserLogin { get; set; }
		public required int RequestRightId { get; set; }
		public User? User { get; set; }
		public RequestRight? RequestRight { get; set; }
	}
}