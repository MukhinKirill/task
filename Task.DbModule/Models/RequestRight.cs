namespace Task.DbModule.Models
{
	public class RequestRight
	{
		public required uint Id { get; set; }
		public required string Name { get; set; }
		public ICollection<UserRequestRight>? UserRequestRights { get; set; }
	}
}