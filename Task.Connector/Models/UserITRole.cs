using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Task.Connector.Models
{
    public class UserITRole
    {
        [Key]
        [Column("roleId")]
        public int RoleId { get; set; }

        [Key]
        [Column("userId")]
        public string UserId { get; set; }
    }
}
