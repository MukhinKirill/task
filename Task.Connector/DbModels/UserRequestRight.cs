﻿using System;
using System.Collections.Generic;

namespace Task.Connector.DbModels;

public partial class UserRequestRight
{
    public string UserId { get; set; } = null!;

    public int RightId { get; set; }
}
