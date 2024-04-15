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

        public UserBuilder(Context context, User user):this(context)
        {
            _user = user;
        }

        public void AddItRole(string role)
        {
            var userRole = _dbContext.ItRole.FirstOrDefault(i => i.Name == role);
            if (userRole != null)
                _user.Roles.Add(userRole);
            else
                throw new ArgumentException($"ItRole {role} not found");
        }

        public void AddRequestRight(string requestRight)
        {
            var right = _dbContext.RequestRight.FirstOrDefault(i => i.Name == requestRight);
            if (right != null)
                _user.RequestRights.Add(right);
            else
                throw new ArgumentException($"Request right {requestRight} not found");
        }

        public void AddPassword(string password)
        {
            var userPassword = new UserPassword() { Password = password };
            _dbContext.Password.Add(userPassword);
            _user.Passwords = userPassword;
        }

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
