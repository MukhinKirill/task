using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TR.Connectors.Api.Entities.DTO.Responses.UserPropertyResponse
{
    public class CreateUserDto : UserPropertyData
    {
        public string? password { get; set; }
    }
}
