using Dapper;
using Microsoft.Extensions.Logging;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using Tasks = System.Threading.Tasks;
using Task.Connector.Abstractions;
using Task.Integration.Data.Models;
using Task.Integration.Data.Models.Models;
using Task.Connector.Enums;
using Task.Connector.Helpers;
using Task.Integration.Data.DbCommon;
using Task.Connector.Constants;
using Task.Integration.Data.DbCommon.DbModels;
using Microsoft.EntityFrameworkCore;
using Task.Connector.Extensions;

namespace Task.Connector.Repositories
{
    internal class PostgreRepository : IAvanpostRepository
    {
        private string _connectionString;
        private DataBaseProvider _dataBaseProvider = DataBaseProvider.POSTGRE;
        private const string _providerName = "POSTGRE";
        private DbContextFactory _contextFactory;
        public void StartUp(string connectionString)
        {
            _connectionString = ConnectionStringParser.GetConnectionString(_dataBaseProvider);
            _contextFactory = new DbContextFactory(_connectionString);
        }
        public PostgreRepository()
        {
            _connectionString = DataBaseConnectionStrings.PostgreConnectionString;
            _contextFactory = new DbContextFactory(_connectionString);
        }
        public bool AddUserPermissions(string userLogin, IEnumerable<string> rightIds)
        {
            if (!rightIds.Any())
                return false;
            using DataContext dataContext = _contextFactory.GetContext(_providerName);
            foreach (var right in rightIds)
            {
                var rightId = right.Split(':');
                if (!rightId.Any()||rightId.Length!=2)
                    continue;
                if (rightId[0].Equals(PermissionsConstants.RequestRightGroupName))
                {
                    if(!dataContext.UserRequestRights.Any(x=>x.RightId== Convert.ToInt32(rightId[1].Trim())))
                    {
                        dataContext.UserRequestRights.Add(new() { RightId = Convert.ToInt32(rightId[1].Trim()), UserId = userLogin });
                    }
                }
                else
                {
                    if (!dataContext.UserITRoles.Any(x => x.RoleId == Convert.ToInt32(rightId[1].Trim())))
                    {
                        dataContext.UserITRoles.Add(new() { RoleId = Convert.ToInt32(rightId[1].Trim()), UserId = userLogin });
                    }
                }
            }
            dataContext.SaveChanges();
            return true;
        }

        public bool CreateUser(UserToCreate user)
        {
            if (user is null||string.IsNullOrEmpty(user.Login))
            {
                return false;
            }
            using DataContext dataContext = _contextFactory.GetContext(_providerName);
            dataContext.Users.Add(ModelMapper.Map(user));
            var password = new Sequrity()
            {
                UserId=user.Login,
                Password=user.HashPassword,
            };
            dataContext.Passwords.Add(password);
            dataContext.SaveChanges();
            //var properties = user.Properties.ToDictionary(x => x.Name, x => x.Value);
            //properties.Add("login", user.Login);
            //var parameters = new DynamicParameters();
            //parameters.AddDynamicParams(properties);
            //using IDbConnection db = new NpgsqlConnection(_connectionString);
            //db.Open();
            //var dbs = db.Database;
            //var columns = string.Join(", ", properties.Keys.Select(x => $"\"{x}\""));
            //var sql = $"INSERT INTO \"TestTaskSchema\".\"User\" ({columns}) VALUES ({string.Join(", ", properties.Keys.Select(key => $"@{key}"))})";
            //db.Execute(sql, parameters);
            return true;
        }

        public IEnumerable<Permission> GetAllPermissions()
        {
            using DataContext dataContext = _contextFactory.GetContext(_providerName);
            var requestRights=dataContext.RequestRights.AsNoTracking();
            var itRoles = dataContext.ITRoles.AsNoTracking();
            return ModelMapper.Map(requestRights).Union(ModelMapper.Map(itRoles));
        }

        public IEnumerable<Property> GetAllProperties()
        {
            var propertiesInfo=typeof(User).GetProperties().Where(x => x.Name != nameof(User.Login));
            var properties = propertiesInfo.Select(x => new Property(x.Name, ""));
            properties=properties.Append(new(nameof(Sequrity.Password), ""));
            return properties;
        }

        public IEnumerable<string> GetUserPermissions(string userLogin)
        {
            using DataContext dataContext = _contextFactory.GetContext(_providerName);
            var permissions = dataContext.UserRequestRights
                .Where(x=>x.UserId==userLogin).Select(x=>x.RightId.ToString())
                .Union(dataContext.UserITRoles.Where(x => x.UserId == userLogin).Select(x=>x.RoleId.ToString()));
            return permissions.ToList()??null!;
        }

        public IEnumerable<UserProperty> GetUserProperties(string userLogin)
        {
            var userProperties = new List<UserProperty>();
            using DataContext dataContext = _contextFactory.GetContext(_providerName);
            var propertiesInfo = typeof(User).GetProperties().Where(x => x.Name != nameof(User.Login));
            var user = dataContext.Users.FirstOrDefault(x=>x.Login==userLogin);
            if (user is null)
                return null!;
            foreach (var property in propertiesInfo)
            {
                userProperties.Add(new(property.Name, property!.GetValue(user)?.ToString()??""));
            }
            return userProperties;
        }

        public bool IsUserExists(string userLogin)
        {
            using DataContext dataContext = _contextFactory.GetContext(_providerName);
            return dataContext.Users.Any(x=>x.Login==userLogin);
        }

        public bool RemoveUserPermissions(string userLogin, IEnumerable<string> rightIds)
        {
            using DataContext dataContext = _contextFactory.GetContext(_providerName);
            foreach (var right in rightIds)
            {
                var rightId = right.Split(':');
                if (!rightId.Any() || rightId.Length != 2)
                    continue;
                if (rightId[0].Equals(PermissionsConstants.RequestRightGroupName))
                {
                    dataContext.UserRequestRights.Remove(new() { RightId= Convert.ToInt32(rightId[1]),UserId=userLogin });
                }
                else
                {
                    dataContext.UserITRoles.Remove(new() { RoleId = Convert.ToInt32(rightId[1]), UserId = userLogin });
                }
            }
            
            dataContext.SaveChanges();
            return true;
        }

        

        public bool UpdateUserProperties(IEnumerable<UserProperty> properties, string userLogin)
        {
            using DataContext dataContext = _contextFactory.GetContext(_providerName);
            var propertiesInfo = typeof(User).GetProperties();
            var user = dataContext.Users.FirstOrDefault(x => x.Login == userLogin);
            if (user is null)
                return false;
            user.ChangeProperties(properties);
            dataContext.SaveChanges();
            return true;
        }
    }
}
