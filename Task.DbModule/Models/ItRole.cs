namespace Task.DbModule.Models
{
	public class ItRole
	{
		public required uint Id { get; set; }
		public required string Name { get; set; }
		public ICollection<UserItRole> UserItRoles { get; set; } = [];
	}
}