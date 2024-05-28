using System;
using System.Collections.Generic;

namespace Task.Connector.ModelsDb;

public partial class Sample
{
    public long Id { get; set; }

    public long OrderId { get; set; }

    public long ContainerId { get; set; }

    public int? Number { get; set; }
}
