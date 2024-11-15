using System;
using System.Collections.Generic;

namespace Task.Connector.Models;

public partial class User
{
    public string Login { get; set; } = null!;

    public string LastName { get; set; } = null!;

    public string FirstName { get; set; } = null!;

    public string MiddleName { get; set; } = null!;

    public string TelephoneNumber { get; set; } = null!;

    public bool IsLead { get; set; }

    public ICollection<Password> Passwords { get; set; } = new List<Password>();

    public List<UserItrole> UserItroles { get; set; } = null!;

    public List<UserRequestRight>  UserRequestRights { get; set; } = null!;
}
