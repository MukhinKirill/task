using System;
using System.Collections.Generic;

namespace Task.Connector.ModelsDb;

public partial class UserPassword
{
    public int Id { get; set; }

    public string UserId { get; set; } = null!;

    public string Password { get; set; } = null!;
}
