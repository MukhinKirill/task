using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Task.Connector.DataBase
{
    [Table("Passwords")]
    public class UserPassword
    {
        public UserPassword() { }

        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("password")]
        [DbItemProperty("password", "password by person")]
        public string Password { get; set; } = string.Empty;

        [Column("userId")]
        public string UserId { get; set; }
        public User User { get; set; }
    }
}
