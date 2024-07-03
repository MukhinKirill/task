using System;
using System.Collections.Generic;

namespace Task.Connector.Entities;

public partial class ItRole
{
    public int id { get; set; }

    public string name { get; set; } = null!;

    public string corporatePhoneNumber { get; set; } = null!;
}
