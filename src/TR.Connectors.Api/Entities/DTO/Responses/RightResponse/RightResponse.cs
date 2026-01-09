using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TR.Connectors.Api.Entities.DTO.Responses.RightResponse
{
    public class RightResponse
    {
        public List<RightResponseData>? data { get; set; }
        public bool success { get; set; }
        public object? errorText { get; set; }
        public int count { get; set; }

    }
}
