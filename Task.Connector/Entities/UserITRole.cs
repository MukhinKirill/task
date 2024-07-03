using System;
using System.Collections.Generic;

namespace Task.Connector.Entities;

public partial class UserITRole
{
    public string userId { get; set; } = null!;

    public int roleId { get; set; }
}
