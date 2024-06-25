using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;

namespace Task.Connector;

public partial class User
{
    public string Login { get; set; } = null!;

    [Comment("Фамилия пользователя.")]
    public string LastName { get; set; } = null!;

    [Comment("Имя пользователя.")]
    public string FirstName { get; set; } = null!;

    [Comment("Отчество пользователя.")]
    public string MiddleName { get; set; } = null!;

    [Comment("Номер телефона пользователя.")]
    public string TelephoneNumber { get; set; } = null!;

    [Comment("Пользователь лид?")]
    public bool IsLead { get; set; }
}
