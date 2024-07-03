using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Task.Integration.Data.Models.Models;

namespace Task.Connector.Services.Interfaces
{
    public interface IPropertyService
    {
        public IEnumerable<Property> GetAllProperties();
        public IEnumerable<UserProperty> GetUserProperties(string userLogin);
        public void UpdateUserProperties(IEnumerable<UserProperty> properties, string userLogin);
    }
}
