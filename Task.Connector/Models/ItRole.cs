using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;

namespace Task.Connector.Models
{
    [Table("ItRole", Schema = "TestTaskSchema")]
    public class ItRole
    {    
        [Column("id")]      
        public int? Id
        {           
            get;
            set;
        }

        [Column("name")]
        public string Name
        {           
            get;
            set;
        }

        [Column("corporatePhoneNumber")]
        public string CorporatePhoneNumber
        {           
            get;
            set;
        }
    }
}
