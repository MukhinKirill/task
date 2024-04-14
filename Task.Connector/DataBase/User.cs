using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Task.Connector.DataBase
{
    public class User
    {

        [Column("login")]
        public string Login {  get; set; }

        [Column("firstName")]
        [DbItemProperty("firstName","first name")]
        public string FirstName { get; set; }

        [Column("middleName")]
        [DbItemProperty("middleName", "middle name")]
        public string MiddleName { get; set; }

        [Column("lastName")]
        [DbItemProperty("lastName", "last name bu person")]
        public string LastName { get; set; }

        [Column("telephoneNumber")]
        [DbItemProperty("telephoneNumber", "telephone by person")]
        public string TelephoneNumber { get; set; }

        [Column("isLead")]
        [DbItemProperty("isLead", "Is the person lead")]
        public bool IsLead { get; set; }

        public List<ItRole> Roles { get; set; } = new();
        public List<RequestRight> RequestRights { get; set; } = new();

        [DbItemProperty()]
        public UserPassword Passwords { get; set; }
    }
}
