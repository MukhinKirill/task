using System;
using System.Collections.Generic;

namespace Task.Connector.ModelsDb;

public partial class Research
{
    public long Id { get; set; }

    public string Name { get; set; } = null!;

    public decimal Price { get; set; }
}
