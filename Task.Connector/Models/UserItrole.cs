using System;
using System.Collections.Generic;

namespace Task.Connector.Models;

public partial class UserItrole
{
    public string UserId { get; set; } = null!;

    public int RoleId { get; set; }
}
