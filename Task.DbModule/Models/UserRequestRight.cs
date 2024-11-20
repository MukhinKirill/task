namespace Task.DbModule.Models
{
	public class UserRequestRight
	{
		public required uint UserId { get; set; }
		public required uint RequestRightId { get; set; }
		public User? User { get; set; }
		public RequestRight? RequestRight { get; set; }
	}
}