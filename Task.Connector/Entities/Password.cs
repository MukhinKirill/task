using System;
using System.Collections.Generic;

namespace Task.Connector.Entities;

public partial class Password
{
    public int id { get; set; }

    public string userId { get; set; } = null!;

    public string password1 { get; set; } = null!;
}
