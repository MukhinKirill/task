using Base.Models.Results;
using Task.Integration.Data.Models.Models;

namespace Connector.Core.Interfaces.DataAccess.Repositories
{
    public interface IRequestRepository
    {
        /// <summary>
        /// Получение всех прав
        /// </summary>
        /// <returns></returns>
        public ListResult<Permission> GetAllRequests();

        /// <summary>
        /// Получение прав для пользователя
        /// </summary>
        /// <param name="userLogin">Логин пользователя</param>
        /// <returns></returns>
        public ListResult<string> GetUserRequests(string userLogin);

        /// <summary>
        /// Добавить прав для пользователя
        /// </summary>
        /// <param name="userLogin">Логин пользователя</param>
        /// <param name="requests">Идентификаторы прав</param>
        /// <returns></returns>
        public Result AddUserRequests(string userLogin, IEnumerable<int> requestIds);

        /// <summary>
        /// Удалить права у пользователя
        /// </summary>
        /// <param name="userLogin">Логин пользователя</param>
        /// <param name="requests">Идентификаторы прав</param>
        /// <returns></returns>
        public Result DeleteUserRequests(string userLogin, IEnumerable<int> requestIds);
    }
}
