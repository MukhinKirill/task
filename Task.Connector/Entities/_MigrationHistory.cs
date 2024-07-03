using System;
using System.Collections.Generic;

namespace Task.Connector.Entities;

public partial class _MigrationHistory
{
    public string MigrationId { get; set; } = null!;

    public string ProductVersion { get; set; } = null!;
}
