using System;
using System.Collections.Generic;

namespace Task.Connector.Models;

public partial class UserRequestRight
{
    public string UserId { get; set; } = null!;
    public int RightId { get; set; } 

    public User User { get; set; } = null!;
    public RequestRight RequestRight { get; set; } = null!;
}
