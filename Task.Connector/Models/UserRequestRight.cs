using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Task.Connector.Models
{
    [Table("UserRequestRight")]
    public class UserRequestRight
    {
        [Key]
        [Column("rightId")]
        public int RightId { get; set; }

        [Key]
        [Column("userId")]
        public string UserId { get; set; }
    }
}
