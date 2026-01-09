using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TR.Connectors.Api.Entities.DTO.Responses.UserPropertyResponse
{
    public class UserPropertyData
    {
        public string? lastName { get; set; }
        public string? firstName { get; set; }
        public string? middleName { get; set; }
        public string? telephoneNumber { get; set; }
        public bool isLead { get; set; }
        public string? login { get; set; }
        public string? status { get; set; }
    }
}
