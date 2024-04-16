using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Task.Connector.DataBase
{
    internal class UserBuilder
    {
        public UserBuilder(Context context)
        {
            _dbContext = context;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="context">Контекст БД</param>
        /// <param name="user">Пользователь, который требует доработки</param>
        public UserBuilder(Context context, User user):this(context)
        {
            _user = user;
        }

        /// <summary>
        /// Добавление It-роли
        /// </summary>
        /// <param name="role">Название роли</param>
        /// <exception cref="ArgumentException">Возникает, если роль не найдена в БД</exception>
        public void AddItRole(string role)
        {
            var userRole = _dbContext.ItRole.FirstOrDefault(i => i.Name == role);
            if (userRole != null)
                _user.Roles.Add(userRole);
            else
                throw new ArgumentException($"ItRole {role} not found");
        }

        /// <summary>
        /// Добавление прав пользователя
        /// </summary>
        /// <param name="requestRight">имя права</param>
        /// <exception cref="ArgumentException">Возникает, если право было не найдено в БД</exception>
        public void AddRequestRight(string requestRight)
        {
            var right = _dbContext.RequestRight.FirstOrDefault(i => i.Name == requestRight);
            if (right != null)
                _user.RequestRights.Add(right);
            else
                throw new ArgumentException($"Request right {requestRight} not found");
        }

        /// <summary>
        /// Добавляет связанный с пользователем пароль
        /// </summary>
        /// <param name="password">Пароль</param>
        public void AddPassword(string password)
        {
            var userPassword = new UserPassword() { Password = password };
            _dbContext.Password.Add(userPassword);
            _user.Passwords = userPassword;
        }

        /// <summary>
        /// Добавляет значение свойства по названию связанного столбца или по названию свойства. Регистронезависимое
        /// </summary>
        /// <param name="property">Название свойства</param>
        /// <param name="value">Значение</param>
        /// <exception cref="ArgumentException">Возникает, если свойство не было найдено или не доступно для записи</exception>
        public void AddProperty(string property, string value) 
        {
            if (!DbItemTools.TrySetDbItemProperty(_user, property, value))
                throw new ArgumentException($"property {property} in {nameof(User)} not exists or not available for edit");
        }

        public User Build() => _user;

        private readonly User _user = new();

        private Context _dbContext;
    }
}
