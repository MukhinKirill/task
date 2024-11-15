using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using Task.Integration.Data.Models.Models;

namespace Task.Connector.Models
{
    [Table("User", Schema = "TestTaskSchema")]
    public class User
    {
        [Column("login")]
        [Description("Login description")]
        public string Login { get; set; }

        [Column("lastName")]
        [Description("LastName description")]
        public string LastName { get; set; }

        [Column("firstName")]
        [Description("FirstName description")]
        public string FirstName { get; set; }

        [Column("middleName")]
        [Description("MiddleName description")]
        public string MiddleName { get; set; }

        [Column("telephoneNumber")]
        [Description("TelephoneNumber description")]
        public string TelephoneNumber { get; set; }

        [Column("isLead")]
        [Description("IsLead description")]
        public bool IsLead { get; set; }

        // Конструктор для создания нового пользователя
        public User(string login, string lastName, string firstName, string middleName, string telephoneNumber, bool isLead)
        {
            Login = login;
            LastName = lastName;
            FirstName = firstName;
            MiddleName = middleName;
            TelephoneNumber = telephoneNumber;
            IsLead = isLead;
        }
    }

}
