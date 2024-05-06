using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Task.Connector.Models
{

	[Table("ItRole")]
	public class ItRole
	{
		[Key]
		[Column("id")]
		public int Id { get; set; }

		[Column("name")]
		public string Name { get; set; }

		[Column("corporatePhoneNumber")]
		public string CorporatePhoneNumber { get; set; }
	}
}
