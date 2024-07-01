using Task.Connector.Parsers.Enums;

namespace Task.Connector.Parsers.Records
{
    public record PermissionId(PermissionTypes Type, int Id)
    {
    }
}
