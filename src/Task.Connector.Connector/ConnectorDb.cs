using Task.Connector.DependencyInjection;
using Task.Integration.Data.Models;
using Task.Integration.Data.Models.Models;

namespace Task.Connector.Connector;

public class ConnectorDb : IConnector
{
    public ILogger Logger { get; set; } = null!;

    /// <summary>
    /// Конфигурация коннектора через строку подключения.
    /// </summary>
    /// <remarks>
    /// Настройки для подключения к ресурсу: строка подключения к бд,<br />
    /// путь к хосту с логином и паролем, дополнительные параметры конфигурации бизнес-логики и тд,<br />
    /// формат любой, например: <see langword="key1=value1; key2=value2"/>.
    /// </remarks>
    /// <param name="connectionString">Строка подключения.</param>
    public void StartUp(string connectionString)
    {
        ServiceLocator.Init(connectionString);
    }

    /// <summary>
    /// Создать пользователя с набором свойств по умолчанию.
    /// </summary>
    /// <param name="user">Модель создания пользователя.</param>
    public void CreateUser(UserToCreate user)
    {
        Logger.Debug("проверка");
    }

    /// <summary>
    /// Проверка существования пользователя.
    /// </summary>
    /// <param name="userLogin">Логин пользователя.</param>
    /// <returns>Вернёт <see langword="true"/>, если пользователь существует, иначе <see langword="false"/>.</returns>
    public bool IsUserExists(string userLogin)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Получение всех свойств пользователя.
    /// </summary>
    /// <returns>Коллекция свойств.</returns>
    public IEnumerable<Property> GetAllProperties() // Метод позволяет получить все свойства пользователя(смотри Описание системы), пароль тоже считать свойством
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Получить все значения свойств пользователяю.
    /// </summary>
    /// <param name="userLogin">Логин пользователя.</param>
    /// <returns>Коллекция значений.</returns>
    public IEnumerable<UserProperty> GetUserProperties(string userLogin)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Обновить значения свойств пользователя.
    /// </summary>
    /// <param name="properties">Коллекция свойств.</param>
    /// <param name="userLogin">Логин пользователя.</param>
    public void UpdateUserProperties(IEnumerable<UserProperty> properties, string userLogin)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Получить все права в системе.
    /// </summary>
    /// <returns>Коллекция прав.</returns>
    public IEnumerable<Permission> GetAllPermissions() //  (смотри Описание системы клиента)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Добавить права пользователю в системе. 
    /// </summary>
    /// <param name="userLogin">Логин пользователя.</param>
    /// <param name="rightIds">Коллекция идентификаторв прав.</param>
    public void AddUserPermissions(string userLogin, IEnumerable<string> rightIds)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Удалить права пользователю в системе.
    /// </summary>
    /// <param name="userLogin">Логин пользователя.</param>
    /// <param name="rightIds">Коллекция идентификаторов прав.</param>
    public void RemoveUserPermissions(string userLogin, IEnumerable<string> rightIds)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Получить права пользователя в системе
    /// </summary>
    /// <param name="userLogin">Логин пользователя.</param>
    /// <returns>Коллекция идентификаторов прав.</returns>
    public IEnumerable<string> GetUserPermissions(string userLogin)
    {
        throw new NotImplementedException();
    }
}