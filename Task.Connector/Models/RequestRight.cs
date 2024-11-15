using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Task.Connector.Models
{
    [Table("RequestRight", Schema = "TestTaskSchema")]
    public class RequestRight
    {      
        [Column("id")]
        public int Id { get; set; }

        [Column("name")]
        public string Name { get; set; }

        public RequestRight()
        {
        
        }

        public RequestRight(int rightId, string rightName)
        {
            Id = rightId;
            Name = rightName;
        }
    }
}
