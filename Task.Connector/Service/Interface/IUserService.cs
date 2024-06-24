using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Task.Connector.Models;
using Task.Integration.Data.DbCommon.DbModels;
using Task.Integration.Data.Models.Models;

namespace Task.Connector.Service.Interface
{
    public interface IUserService
    {
        PropertyModel ParseProperties(IEnumerable<UserProperty> userProperties);

        IEnumerable<UserProperty> SerializeUserPropertyFromUser(User entity);
    }
}
