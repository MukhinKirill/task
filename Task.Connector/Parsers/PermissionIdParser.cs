using System.Text.RegularExpressions;
using Task.Connector.Parsers.Enums;
using Task.Connector.Parsers.Records;

namespace Task.Connector.Parsers
{
    public class PermissionIdParser : IStringParser<PermissionId>
    {
        private readonly string _requestRightGroupName;
        private readonly string _itRoleRightGroupName;
        //private readonly string _delimeter;

        public PermissionIdParser(string requestRightGroupName, string itRoleRightGroupName)
        {
            _requestRightGroupName = requestRightGroupName;
            _itRoleRightGroupName = itRoleRightGroupName;
            //_delimeter = delimeter;
        }

        public PermissionId Parse(string input)
        {
            var match = new Regex(@"(\w+[^\W])\W*(\d+)").Match(input);

            return new PermissionId(GetPermissionType(match.Groups[1].Value), Int32.Parse(match.Groups[2].Value));
        }

        private PermissionTypes GetPermissionType(string input)
        {
            if(input == _requestRightGroupName)
            {
                return PermissionTypes.RequestRight;
            }
            else if(input == _itRoleRightGroupName)
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
