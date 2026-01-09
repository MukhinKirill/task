using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TR.Connectors.Api.Entities.DTO.Responses.RoleResponse
{
    public class RoleResponseData
    {
        public int id { get; set; }
        public string? name { get; set; }
        public string? corporatePhoneNumber { get; set; }
    }
}
