using System;
using System.Collections.Generic;

namespace Task.Connector.Models;

public partial class ItRole
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public string CorporatePhoneNumber { get; set; } = null!;
}
