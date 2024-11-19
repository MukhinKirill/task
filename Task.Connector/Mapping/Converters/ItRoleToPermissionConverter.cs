using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Task.Integration.Data.DbCommon.DbModels;
using Task.Integration.Data.Models.Models;

namespace Task.Connector.Mapping.Converters
{
    internal class ItRoleToPermissionConverter : ITypeConverter<ITRole, Permission>
    {
        public Permission Convert(ITRole sourceItRole, Permission destinationPermission, ResolutionContext context)
        {
            destinationPermission = new Permission(sourceItRole.Id.ToString(), sourceItRole.Name ?? string.Empty, string.Empty);
            return destinationPermission;
        }
    }
}
