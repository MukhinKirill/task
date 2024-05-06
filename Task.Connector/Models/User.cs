using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;

namespace Task.Connector.Models
{
	[Table("User")]
	public class User
	{
        [Key]
		[Column("login")]
		public string Login { get; set; }

		[Column("lastName")]
		[Display(Description = "Фамилия")]
		public string LastName { get; set; }

		[Column("firstName")]
		[Display(Description = "Имя")]
		public string FirstName { get; set; }

		[Column("middleName")]
		[Display(Description = "Отчество")]
		public string MiddleName { get; set; }

		[Column("telephoneNumber")]
		[Display(Description = "Телефон")]
		public string TelephoneNumber { get; set; }

		[Column("isLead")]
		[Display(Description = "Признак: руководитель")]
		public bool IsLead { get; set; }


	}
}
