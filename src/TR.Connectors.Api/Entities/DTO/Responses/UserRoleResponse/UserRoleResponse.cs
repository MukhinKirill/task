using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TR.Connectors.Api.Entities.DTO.Responses.RoleResponse;

namespace TR.Connectors.Api.Entities.DTO.Responses.UserRoleResponse
{
    public class UserRoleResponse
    {
        public List<RoleResponseData>? data { get; set; }
        public bool success { get; set; }
        public object? errorText { get; set; }
        public int count { get; set; }
    }
}
