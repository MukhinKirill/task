using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Task.Connector.Models
{
    [Table("User")]
    public class User
    {
        [Key]
        [Column("login")]
        public string Login { get; set; }

        [Required]
        [Column("lastName")]
        public string LastName { get; set; }

        [Required]
        [Column("firstName")]
        public string FirstName { get; set; }

        [Required]
        [Column("middleName")]
        public string MiddleName { get; set; }

        [Required]
        [Column("telephoneNumber")]
        public string TelephoneNumber { get; set; }

        [Required]
        [Column("isLead")]
        public bool IsLead { get; set; }
    }
}
