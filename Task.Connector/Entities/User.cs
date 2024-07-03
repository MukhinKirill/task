using System;
using System.Collections.Generic;

namespace Task.Connector.Entities;

public partial class User
{
    public string login { get; set; } = null!;

    public string lastName { get; set; } = null!;

    public string firstName { get; set; } = null!;

    public string middleName { get; set; } = null!;

    public string telephoneNumber { get; set; } = null!;

    public bool isLead { get; set; }
}
