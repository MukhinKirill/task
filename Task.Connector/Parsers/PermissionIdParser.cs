using System.Text.RegularExpressions;
using Task.Connector.Parsers.Enums;
using Task.Connector.Parsers.Records;

namespace Task.Connector.Parsers
{
    public class PermissionIdParser : IStringParser<PermissionId>
    {
        private readonly PermissionParserConfiguration _configuration;

        public PermissionIdParser(ConfigureManager configureManager)
        {
            _configuration = configureManager.PermissionParserConfiguration;
        }

        public PermissionId Parse(string input)
        {
            var match = new Regex(@"(\w+[^\W])\W*(\d+)").Match(input);

            return new PermissionId(GetPermissionType(match.Groups[1].Value), Int32.Parse(match.Groups[2].Value));
        }

        private PermissionTypes GetPermissionType(string input)
        {
            if(input == _configuration.RequestRightGroupName)
            {
                return PermissionTypes.RequestRight;
            }
            else if(input == _configuration.ItRoleRightGroupName)
            {
                return PermissionTypes.ItRole;
            }
            else
            {
                return PermissionTypes.Undefined;
            }
        }
    }
}
