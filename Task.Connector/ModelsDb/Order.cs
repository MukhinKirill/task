using System;
using System.Collections.Generic;

namespace Task.Connector.ModelsDb;

public partial class Order
{
    public long Id { get; set; }

    public DateTime CreatedDate { get; set; }

    public string? Fio { get; set; }
}
