using Base.Models.Results;
using Task.Integration.Data.Models.Models;

namespace Connector.Core.Interfaces.DataAccess.Repositories
{
    public interface IUserRepository
    {
        /// <summary>
        /// Создание нового пользователя
        /// </summary>
        /// <param name="user">Пользователь</param>
        /// <returns></returns>
        public Result CreateUser(UserToCreate user);

        /// <summary>
        /// Получение всех полей
        /// </summary>
        /// <returns></returns>
        public ListResult<Property> GetAllProperties();

        /// <summary>
        /// Получение всех полей пользователя
        /// </summary>
        /// <param name="userLogin">Логин пользователя</param>
        /// <returns></returns>
        public ListResult<UserProperty> GetUserProperties(string userLogin);

        /// <summary>
        /// Проверка на существование пользователя
        /// </summary>
        /// <param name="userLogin">Логин пользователя</param>
        /// <returns></returns>
        public Result<bool> IsUserExists(string userLogin);

        /// <summary>
        /// Изменение пользователя
        /// </summary>
        /// <param name="userLogin">Логин пользователя</param>
        /// <param name="properties">Поля для изменения</param>
        /// <returns></returns>
        public Result UpdateUser(string userLogin, IEnumerable<UserProperty> properties);
    }
}
