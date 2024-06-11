using Base.Models.Results;
using Task.Integration.Data.Models.Models;

namespace Connector.Core.Interfaces.DataAccess.Repositories
{
    public interface IRoleRepository
    {
        /// <summary>
        /// Получение всех ролей
        /// </summary>
        /// <returns></returns>
        public ListResult<Permission> GetAllRoles();

        /// <summary>
        /// Получение ролей для пользователя
        /// </summary>
        /// <param name="userLogin">Логин пользователя</param>
        /// <returns></returns>
        public ListResult<string> GetUserRoles(string userLogin);

        /// <summary>
        /// Добавить роли для пользователя
        /// </summary>
        /// <param name="userLogin">Логин пользователя</param>
        /// <param name="requests">Идентификаторы ролей</param>
        /// <returns></returns>
        public Result AddUserRoles(string userLogin, IEnumerable<int> roleIds);

        /// <summary>
        /// Удалить роли у пользователя
        /// </summary>
        /// <param name="userLogin">Логин пользователя</param>
        /// <param name="requests">Идентификаторы ролей</param>
        /// <returns></returns>
        public Result DeleteUserRoles(string userLogin, IEnumerable<int> roleIds);
    }
}
