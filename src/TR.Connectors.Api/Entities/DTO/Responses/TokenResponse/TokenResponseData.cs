using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TR.Connectors.Api.Entities.DTO.Responses.TokenResponse
{
    public class TokenResponseData
    {
        public string? access_token { get; set; }
        public int expires_in { get; set; }
    }
}
