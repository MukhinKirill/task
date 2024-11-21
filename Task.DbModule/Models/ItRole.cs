namespace Task.DbModule.Models
{
	public class ITRole
	{
		public required int Id { get; set; }
		public required string Name { get; set; }
		public required string CorporatePhoneNumber { get; set; }
		public ICollection<UserITRole>? UserITRoles { get; set; }
	}
}