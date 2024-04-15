using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Task.Connector.DataBase
{
    public class User
    {
        [Column("login")]
        public string Login {  get; set; } = string.Empty;

        [Column("firstName")]
        [DbItemProperty("firstName","first name")]
        public string FirstName { get; set; } = string.Empty;

        [Column("middleName")]
        [DbItemProperty("middleName", "middle name")]
        public string MiddleName { get; set; } = string.Empty;

        [Column("lastName")]
        [DbItemProperty("lastName", "last name bu person")]
        public string LastName { get; set; } = string.Empty;

        [Column("telephoneNumber")]
        [DbItemProperty("telephoneNumber", "telephone by person")]
        public string TelephoneNumber { get; set; } = string.Empty;

        [Column("isLead")]
        [DbItemProperty("isLead", "Is the person lead")]
        public bool IsLead { get; set; }

        public List<ItRole> Roles { get; set; } = new();
        public List<UserITRole> UserRoles { get; set; } = new();


        public List<RequestRight> RequestRights { get; set; } = new();
        public List<UserRequestRight> UserRight { get; set; } = new();

        [DbItemProperty()]
        public UserPassword Passwords { get; set; }
    }
}
