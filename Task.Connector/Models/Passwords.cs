using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Task.Connector.Models
{
    public class Passwords
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Required]
        [Column("userId")]
        public string UserId { get; set; }

        [Required]
        [Column("password")]
        public string Password { get; set; }
    }
}
