using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TR.Connectors.Api.Entities.DTO.Responses.TokenResponse
{
    public class TokenResponse
    {
        public TokenResponseData? data { get; set; }
        public bool success { get; set; }
        public object? errorText { get; set; }
        public object? count { get; set; }
    }
}
