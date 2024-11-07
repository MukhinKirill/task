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
}
