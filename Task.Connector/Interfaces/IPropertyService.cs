using Task.Integration.Data.Models.Models;

namespace AvanpostGelik.Connector.Interfaces;

public interface IPropertyService
{
    IEnumerable<Property> GetAllProperties();
    IEnumerable<UserProperty> GetUserProperties(string userLogin);
}
