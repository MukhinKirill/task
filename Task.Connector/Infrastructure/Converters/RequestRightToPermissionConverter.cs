using Task.Integration.Data.DbCommon.DbModels;
using Task.Integration.Data.Models.Models;

namespace Task.Connector.Infrastructure.Converters;

public class RequestRightToPermissionConverter : IModelConverter<RequestRight, Permission>
{
    // TODO make sure Request right and ItRole ids don't clash
    public Permission Convert(RequestRight tIn)
    {
        // Why is the id a string????
        return new Permission(tIn.Id.ToString()!, tIn.Name, tIn.Name);
    }
}