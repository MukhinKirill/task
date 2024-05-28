using System;
using System.Collections.Generic;

namespace Task.Connector.ModelsDb;

public partial class Rule
{
    public long ResearcheId { get; set; }

    public long ContainerId { get; set; }

    public long Id { get; set; }
}
