using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Task.Connector.Models
{
    public class MigrationHistory
    {
        [Key]
        public string MigrationId { get; set; }
        [Required]
        public string ProductVersion { get; set; }
    }
}
