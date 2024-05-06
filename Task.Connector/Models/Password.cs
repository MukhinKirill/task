using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Task.Connector.Models
{
	public class Password
	{
		[Key]
		[Column("id")]
		public int Id { get; set; }

		[Column("userId")]
		public string UserId { get; set; }

		[Column("password")]
		[Display(Description = "Пароль")]
		public string PasswordValue { get; set; }

		public Password() {

		}

		public Password(string userId, string password) {
			UserId = userId;
			PasswordValue = password;
		}
	}
}
