using Task.Connector.Infrastructure.DataModels;

namespace Task.Connector.Infrastructure.Repository.Interfaces;

/// <summary>
/// Репозиторий пользователей.
/// </summary>
public interface IUserRepository
{
    /// <summary>
    /// Создать пользователя.
    /// </summary>
    /// <param name="user">Пользователь.</param>
    public void Create(UserDataModel user);
    /// <summary>
    /// Проверить наличие пользователя в системе.
    /// </summary>
    /// <param name="login">Логин пользователя.</param>
    /// <returns>Наличие пользователя bool.</returns>
    public bool IsExists(string login);
    /// <summary>
    /// Получить пользователя.
    /// </summary>
    /// <param name="login">Логин пользователя.</param>
    /// <returns>Пользователь UserDataModel?.</returns>
    public UserDataModel? GetUserModelByLogin(string login);
    /// <summary>
    /// Обновить пользователя в системе.
    /// </summary>
    /// <param name="user">Пользователь.</param>
    public void Update(UserDataModel user);
}