
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Task.Connector.Models
{
	[PrimaryKey(nameof(UserId), nameof(RoleId))]
	[Table("UserITRole")]
	public class UserITRole
	{
		[Key]
		[Column("userId")]
		public string UserId { get; set; }

		[Column("roleId")]
		public int RoleId { get; set; }
	}
}
