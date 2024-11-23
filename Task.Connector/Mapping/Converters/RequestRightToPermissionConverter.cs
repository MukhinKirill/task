using AutoMapper;
using Task.Integration.Data.DbCommon.DbModels;
using Task.Integration.Data.Models.Models;

namespace Task.Connector.Mapping.Converters
{
    internal class RequestRightToPermissionConverter : ITypeConverter<RequestRight, Permission>
    {
        public Permission Convert(RequestRight sourceRight, Permission destinationPermission, ResolutionContext context)
        {
            destinationPermission = new Permission(sourceRight.Id.ToString(), sourceRight.Name ?? string.Empty, string.Empty);
            return destinationPermission;
        }
    }
}