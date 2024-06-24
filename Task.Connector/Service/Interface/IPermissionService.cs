using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Task.Integration.Data.DbCommon.DbModels;

namespace Task.Connector.Service.Interface
{
    public interface IPermissionService
    {
        void ParsePermission(IEnumerable<string> rightIds, out IEnumerable<int> outRequestsRightIds, out IEnumerable<int> outItRoleIds);
    }
}
