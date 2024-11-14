using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Task.Integration.Data.DbCommon.DbModels;

namespace Task.Connector
{
    internal static class LC
    {
        internal static readonly Dictionary<string, string> PROVIDERS = new()
        {
            { "MSSQL","sqlserver" },
            {"POSTGRE","postgresql" }
        };

        internal static readonly string[] NOT_PROPS = new[] 
        { 
            $"{typeof(Sequrity).Name}-Id", 
            $"{typeof(Sequrity).Name}-UserId",
            $"{typeof(User).Name}-Login",
            $"{typeof(User).Name}-Id"  
        };

        internal static readonly string requestRightGroupName = "Request";
        internal static readonly string itRoleRightGroupName = "Role";
    }
}
