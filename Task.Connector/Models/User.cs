using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection;
using System.Runtime.CompilerServices;
using Task.Connector.Attributes;
using Task.Integration.Data.Models.Models;

namespace Task.Connector.Models;

public partial class User
{
    public string Login { get; set; } = null!;

    [Property("lastName", "Фамилия", DefaultValue = "Иванов")]
    public string LastName { get; set; } = null!;

    [Property("firstName", "Имя", DefaultValue = "Иван")]
    public string FirstName { get; set; } = null!;

    [Property("middleName", "Отчество", DefaultValue = "Иванович")]
    public string MiddleName { get; set; } = null!;

    [Property("telephoneNumber", "Номер телефона", DefaultValue = "89000000000")]
    public string TelephoneNumber { get; set; } = null!;

    [Property("isLead", "Наличие лидерства", DefaultValue = "false")]
    public bool IsLead { get; set; }

}
