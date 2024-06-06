namespace Task.Connector.Models;

internal partial class MigrationHistory
{
    public string MigrationId { get; set; } = null!;

    public string ProductVersion { get; set; } = null!;
}
