using AutoMapper;
using Task.Connector.Mapping.Converters;
using Task.Integration.Data.DbCommon.DbModels;
using Task.Integration.Data.Models.Models;

namespace Task.Connector.Mapping;

public class TaskMappingProfile : Profile
{
    public TaskMappingProfile()
    {
        CreateMap<UserToCreate, User>().ConvertUsing<UserConverter>();

        CreateMap<RequestRight, Permission>().ConvertUsing<RequestRightToPermissionConverter>();

        CreateMap<ITRole, Permission>().ConvertUsing<ItRoleToPermissionConverter>();
    }
}