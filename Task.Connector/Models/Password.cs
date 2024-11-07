using System;
using System.Collections.Generic;

namespace Task.Connector.Models;

public partial class Password
{
    public int Id { get; set; }

    public string UserId { get; set; } = null!;

    public string Password1 { get; set; } = null!;
}
