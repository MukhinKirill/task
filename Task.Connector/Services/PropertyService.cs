using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Task.Connector.Helpers.Property;
using Task.Integration.Data.DbCommon.DbModels;
using Task.Integration.Data.Models.Models;

namespace Task.Connector.Services
{
    public interface IPropertyService
    {
        IEnumerable<Property> GetAllProperties();
    }
    internal class PropertyService : IPropertyService
    {

        public IEnumerable<Property> GetAllProperties()
        {
            var result = PropertyHelper.GetProperties(typeof(User), "Login")
                .Concat(PropertyHelper.GetProperties(typeof(Sequrity), "Id", "UserId"));
            return result;
        }
    }
}
