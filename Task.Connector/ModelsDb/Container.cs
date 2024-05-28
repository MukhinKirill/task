using System;
using System.Collections.Generic;

namespace Task.Connector.ModelsDb;

public partial class Container
{
    public long Id { get; set; }

    public string Name { get; set; } = null!;
}
