using Task.Integration.Data.Models.Models;

namespace Task.Connector.Models
{
    internal class UserObjectCreateParamaters : UserModel
    {
        public UserObjectCreateParamaters(string login, IEnumerable<UserProperty> userProperties) : base(login, userProperties)
        {
        }
    }
}
