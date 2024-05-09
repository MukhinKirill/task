using Task.Connector.Domain.Models;

namespace Task.Connector.Infrastructure.DataModels;

public class PermissionDataModel
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public string Type { get; set; }
}