using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TR.Connectors.Api.Entities.DTO.Responses.UserResponse
{
    public class UserResponse
    {
        public List<UserResponseData>? data { get; set; }
        public bool success { get; set; }
        public object? errorText { get; set; }
        public int count { get; set; }
    }
}
