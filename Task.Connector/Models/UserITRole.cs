using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Task.Connector.Models
{
    [Table("UserITRole", Schema = "TestTaskSchema")]
    public class UserITRole
    {
        [Column("userId")]
        public string UserId { get; set; }

        [Column("roleId")]
        public int RoleId { get; set; }
    }
}
