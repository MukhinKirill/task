using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TR.Connectors.Api.Enums;

namespace TR.Connectors.Api.Interfaces
{
    public interface IAccessTypeParser
    {
        bool TryParse(string prefixed, out AccessType type, out string id);
        string Format(AccessType type, string id);
    }
}
