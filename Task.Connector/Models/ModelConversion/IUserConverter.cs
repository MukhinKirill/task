using Task.Integration.Data.Models.Models;

using UserObj = System.Collections.Generic.Dictionary<string, object>;

namespace Task.Connector.Models.ModelConversion
{
    // В этом интерфейса пользователи представлены как property bag
    // в формате Dictionary<string, string>

    public interface IUserConverter
    {
        IEnumerable<UserProperty> ExtractProperties(UserObj properties);

        // Код предполагает, что Description описывает тип свойства в базе данных
        IEnumerable<Property> ConvertProperties(Dictionary<string, string> properties);

        UserObj ConvertUserToCreate(UserToCreate user);

        UserObj ConstructUser(IEnumerable<UserProperty> properties, string userLogin);
    }
}
