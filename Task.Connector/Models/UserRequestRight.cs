using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Task.Connector.Models
{
    [Table("UserRequestRight", Schema = "TestTaskSchema")]
    public class UserRequestRight
    {
        [Column("userId")]
        public string UserId { get; set; }

        [Column("rightId")]
        public int RightId { get; set; }
    }
}
