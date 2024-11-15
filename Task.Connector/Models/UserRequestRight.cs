using System;
using System.Collections.Generic;

namespace Task.Connector.Models;

public partial class UserRequestRight
{
    public User UserId { get; set; } = null!;

    public List<RequestRight> RightId { get; set; }
}
