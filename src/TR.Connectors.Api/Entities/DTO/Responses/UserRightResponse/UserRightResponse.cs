using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TR.Connectors.Api.Entities.DTO.Responses.RightResponse;

namespace TR.Connectors.Api.Entities.DTO.Responses.UserRightResponse
{
    public class UserRightResponse
    {
        public List<RightResponseData>? data { get; set; }
        public bool success { get; set; }
        public object? errorText { get; set; }
        public int count { get; set; }
    }
}
