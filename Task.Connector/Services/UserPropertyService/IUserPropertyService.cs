using Task.Integration.Data.Models.Models;

namespace Task.Connector.Services.UserPropertyService;

internal interface IUserPropertyService
{
    IEnumerable<UserProperty> GetUserProperties(string userLogin);
    void UpdateUserProperties(IEnumerable<UserProperty> properties, string userLogin);
}
