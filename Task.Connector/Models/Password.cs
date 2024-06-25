using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;

namespace Task.Connector;

public partial class Password
{
    public int Id { get; set; }

    public string UserId { get; set; } = null!;

    [Comment("Пароль пользователя.")]
    public string Password1 { get; set; } = null!;
}
