using System;
using System.Collections.Generic;

namespace Task.Connector.Entities;

public partial class UserRequestRight
{
    public string userId { get; set; } = null!;

    public int rightId { get; set; }
}
