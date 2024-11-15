using System;
using System.Collections.Generic;

namespace Task.Connector.Models;

public partial class UserItrole
{
    public User UserId { get; set; } = null!;

    public List<ItRole> RoleId { get; set; } = null;
}
