
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Task.Connector.Models
{
	[PrimaryKey(nameof(UserId), nameof(RightId))]
	[Table("UserRequestRight") ]
	public class UserRequestRight
	{
		[Key]
		[Column("userId", Order = 1)]
		public string UserId { get; set; }

		[Key]
		[Column("rightId", Order = 2)]
		public int RightId { get; set; }
	}
}
