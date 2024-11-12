using Task.Integration.Data.DbCommon.DbModels;
using Task.Integration.Data.Models.Models;

namespace Task.Connector.Infrastructure.Converters;

public class ItRoleToPermissionConverter : IModelConverter<ITRole, Permission>
{
    // TODO make sure Request right and ItRole ids don't clash
    public Permission Convert(ITRole tIn)
    {
        return new Permission(tIn.Id.ToString()!, tIn.Name, tIn.Name);
    }
}