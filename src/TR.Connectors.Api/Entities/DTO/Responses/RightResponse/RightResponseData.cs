using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TR.Connectors.Api.Entities.DTO.Responses.RightResponse
{
    public class RightResponseData
    {
        public int id { get; set; }
        public string? name { get; set; }
        public object? users { get; set; }
    }
}
