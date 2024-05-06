using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Task.Connector.Models
{
	[Table("RequestRight")]
	public class RequestRight
	{
		[Key]
		[Column("id")]
		public int Id { get; set; }

		[Column("name")]
		public string Name { get; set; }
	}
}
