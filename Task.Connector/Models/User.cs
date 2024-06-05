using Task.Connector.Attributes;

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
