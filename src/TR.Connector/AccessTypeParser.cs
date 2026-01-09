using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TR.Connectors.Api.Enums;
using TR.Connectors.Api.Interfaces;

namespace TR.Connector
{
    public class AccessTypeParser : IAccessTypeParser
    {
        public bool TryParse(string prefixed, out AccessType type, out string id)
        {
            type = default;
            id = string.Empty;
            if (string.IsNullOrWhiteSpace(prefixed)) return false;

            var parts = prefixed.Split(',', 2);
            if (parts.Length != 2) return false;

            id = parts[1].Trim();
            var key = parts[0].Trim();

            if (Enum.TryParse<AccessType>(key, ignoreCase: true, out var parsed))
            {
                type = parsed;
                return true;
            }

            return false;
        }
        public string Format(AccessType type, string id) => $"{type},{id}";
    }
}
