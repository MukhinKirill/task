using System;
using System.Collections.Generic;

namespace Task.Connector.Models;

public partial class MigrationHistory
{
    public string MigrationId { get; set; } = null!;

    public string ProductVersion { get; set; } = null!;
}
