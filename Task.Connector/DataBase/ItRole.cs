using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Task.Connector.DataBase
{
    public class ItRole
    {
        [Column("id")]
        public int Id { get; set; }

        [Column("name")]
        public string Name { get; set; } = string.Empty;

        [Column("corporatePhoneNumber")]
        public string CorporatePhoneNumber { get; set; } = string.Empty;

        public List<User> Users { get; set; } = new();
        public List<UserITRole> UserRoles { get; set; } = new();

    }
}
