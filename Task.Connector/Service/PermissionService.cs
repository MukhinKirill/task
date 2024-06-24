using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Task.Connector.Service.Interface;
using Task.Integration.Data.DbCommon.DbModels;

namespace Task.Connector.Service
{
    public class PermissionService : IPermissionService
    {
        private string _requestRightGroupName = "Request";
        private string _itRoleRightGroupName = "Role";
        private string delimeter = ":";
        public void ParsePermission(IEnumerable<string> rightIds, out IEnumerable<int> outRequestsRightIds, out IEnumerable<int> outItRoleIds)
        {
            outRequestsRightIds = new List<int>();
            outItRoleIds = new List<int>();
            foreach (var rightId in rightIds)
            {
                var parsePermission = rightId.Split(delimeter);
                var permissionType = parsePermission[0];
                int permission;
                if (!int.TryParse(parsePermission[1], out permission))
                    throw new ArgumentException("The permission id could not be converted");

                if (permissionType == _requestRightGroupName)
                {
                    ((List<int>)outRequestsRightIds).Add(permission);
                }
                else if (permissionType == _itRoleRightGroupName)
                {
                    ((List<int>)outItRoleIds).Add(permission);
                }
                else
                {
                    throw new ArgumentException($"{permissionType} there is no type of permission");
                }
            }


        }
    }
}
