using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics.CodeAnalysis;

namespace Task.Connector.Models
{
    [Table("Passwords", Schema = "TestTaskSchema")]
    public class Password
    {
        [Key]
        [Column("id")]

        public int Id 
        {
            [return: NotNull]
            get;
            set;
        }

        [Column("userId")]
        [MaxLength(22)]
        public string UserId
        {
            [return: NotNull]
            get; 
            set;
        }

        [Column("password")]
        [MaxLength(20)]
        public string hashPassword
        {
            [return: NotNull]
            get;
            set;
        }

        public Password() { }

        public Password(string userId, string password)
        {
            UserId = userId;
            hashPassword = password;
        }
    }
}
