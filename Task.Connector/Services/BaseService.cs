using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Task.Integration.Data.DbCommon;

namespace Task.Connector.Services
{
    internal class BaseService
    {
        public readonly DataContext Context;
        public BaseService(DataContext context) {  Context = context; }
    }
}
