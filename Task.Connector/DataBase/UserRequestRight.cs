using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Task.Connector.DataBase
{
    public class UserRequestRight
    {
        [Column("userId")]
        public string UserId { get; set; }
        public User User { get; set; }

        [Column("rightId")]
        public int RightId { get; set; }
        public RequestRight Right { get; set; }
    }
}
