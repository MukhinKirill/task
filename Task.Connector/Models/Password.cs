using System;
using System.Collections.Generic;
using Task.Connector.Attributes;
using Task.Integration.Data.Models.Models;

namespace Task.Connector.Models;

public partial class Password
{
    public int Id { get; set; }

    public string UserId { get; set; } = null!;

    [Property("password", "Пароль пользователя")]
    public string Password1 { get; set; } = null!;
}
