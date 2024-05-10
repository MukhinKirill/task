using Task.Connector.Domain.Models;
using Task.Connector.Infrastructure.DataModels;
using Task.Integration.Data.Models.Models;

namespace Task.Connector.Infrastructure.Repository.Interfaces;

/// <summary>
/// Репозиторий прав.
/// </summary>
public interface IPermissionRepository
{
    /// <summary>
    /// Получить все права в системе.
    /// </summary>
    /// <returns>Права пользователя IEnumerable&lt;Permission&gt;.</returns>
    public IEnumerable<Permission> GetAllPermissions();
    /// <summary>
    /// Получить права пользователя в системе.
    /// </summary>
    /// <param name="login">Логин пользователя.</param>
    /// <returns>Права пользователя IEnumerable&lt;PermissionDataModel&gt;.</returns>
    public IEnumerable<PermissionDataModel> GetUserPermissions(string login);
    /// <summary>
    /// Добавить роль.
    /// </summary>
    /// <param name="permissions">Роли пользователя.</param>
    public void AddRolePermissions(IEnumerable<UserItRole> permissions);
    /// <summary>
    /// Добавить утверждение.
    /// </summary>
    /// <param name="permissions">Утверждения пользователя.</param>
    public void AddRequestPermissions(IEnumerable<UserRequestRight> permissions);
    /// <summary>
    /// Удалить роль.
    /// </summary>
    /// <param name="permissions">Роли пользователя.</param>
    public void RemoveRequestPermissions(IEnumerable<UserRequestRight> permissions);
    /// <summary>
    /// Удалить утверждение.
    /// </summary>
    /// <param name="permissions">Утверждения пользователя.</param>
    public void RemoveRolePermissions(IEnumerable<UserItRole> permissions);
}