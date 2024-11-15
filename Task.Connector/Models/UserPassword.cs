using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Task.Connector.Models
{
    [Table("Passwords", Schema = "TestTaskSchema")]
    public class UserPassword
    {
        [Column("id")]
        public int Id { get; set; }

        [Column("userId")]
        public required string UserId { get; set; }

        [Column("password")]   
        public required string Password { get; set; }
    }
}
