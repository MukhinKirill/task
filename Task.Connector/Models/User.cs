using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using Task.Integration.Data.Models.Models;

namespace Task.Connector.Models;

public partial class User
{
    public string Login { get; set; } = null!;

    public string LastName { get; set; } = null!;

    public string FirstName { get; set; } = null!;

    public string MiddleName { get; set; } = null!;

    public string TelephoneNumber { get; set; } = null!;

    public bool IsLead { get; set; }

    public static User AddUser(UserToCreate usr)
    {
        var user = new User()
        {
            Login = usr.Login,
            FirstName = usr.Properties.Where(u => u.Name == "FirstName")?.FirstOrDefault()?.Value,
            MiddleName = usr.Properties.Where(u => u.Name == "MiddleName")?.FirstOrDefault()?.Value,
            LastName = usr.Properties.Where(u => u.Name == "LastName")?.FirstOrDefault()?.Value,
            TelephoneNumber = usr.Properties.Where(u => u.Name == "TelephoneNumber")?.FirstOrDefault()?.Value,
            IsLead = usr.Properties.Where(u => u.Name == "isLead")?.FirstOrDefault()?.Value == "true",
        };
        if (user.LastName == null) user.LastName = "Иванов";
        if (user.FirstName == null) user.FirstName = "Иван";
        if (user.MiddleName == null) user.MiddleName = "Иванович";
        if (user.TelephoneNumber == null) user.TelephoneNumber = "89000000000";

        return user;
    }
}
