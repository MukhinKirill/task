using Microsoft.EntityFrameworkCore;
using Task.Connector.DataContext;
using Task.Connector.Exceptions;
using Task.Connector.Models;
using Task.Integration.Data.Models;
using Task.Integration.Data.Models.Models;

namespace Task.Connector
{
    public class ConnectorDb : IConnector
    {
        private  TestContext _dbContext;
        
        public void StartUp(string connectionString)
        {
           
            int startIndex = connectionString.IndexOf("ConnectionString='") + "ConnectionString='".Length;

            
            int endIndex = connectionString.IndexOf("'", startIndex);

            
            var res  = connectionString.Substring(startIndex, endIndex - startIndex);

            _dbContext = new TestContext(res);
        }

        public void CreateUser(UserToCreate user)
        {
            using(var trans = _dbContext.Database.BeginTransaction())
            {
                try
                {
                    var reguser = new User
                    {
                        Login = user.Login,
                        FirstName = user.Properties.FirstOrDefault(x => x.Name == "FirstName")?.Value ?? string.Empty,
                        LastName = user.Properties.FirstOrDefault(x => x.Name == "LastName")?.Value ?? string.Empty,
                        MiddleName = user.Properties.FirstOrDefault(x => x.Name == "MiddleName")?.Value ?? string.Empty,
                        TelephoneNumber = user.Properties.FirstOrDefault(x => x.Name == "TelephoneNumber")?.Value ?? string.Empty,
                        IsLead = user.Properties.FirstOrDefault(x=>x.Name == "IsLead")?.Value == "false" ? false : true
                        
                    };
                    _dbContext.Add(reguser);
                    _dbContext.SaveChanges();
                    var regpass = new Password
                    {
                        UserId = reguser.Login,
                        Password1 = user.HashPassword
                    };

                    _dbContext.Add(regpass);
                    _dbContext.SaveChanges();
                    trans.Commit();
                }
                catch(Exception ex)
                {
                    trans.Rollback();
                    Logger.Error(ex.Message);
                }
                
            }
        }

        public IEnumerable<Property> GetAllProperties()
        {
            var result = new List<Property>();
            try
            {
                var userProps = typeof(User).GetProperties().Where(x => x.Name != "Login").ToList();
                var passwordProps = typeof(Password).GetProperties().Where(x => x.Name != "Id" && x.Name != "UserId").ToList();

                result = userProps.Concat(passwordProps).Select(x => new Property(x.Name, x.PropertyType.ToString())).ToList();
                
            }
            catch(Exception ex)
            {
                Logger.Error(ex.Message);
            }

            return result;
        }

        public IEnumerable<UserProperty> GetUserProperties(string userLogin)
        {
            var result = new List<UserProperty>();
            try
            {
                var user = _dbContext.Users.FirstOrDefault(x => x.Login == userLogin);
                result.Add(new UserProperty(nameof(User.FirstName), user?.FirstName ?? string.Empty));
                result.Add(new UserProperty(nameof(User.LastName), user?.LastName ?? string.Empty));
                result.Add(new UserProperty(nameof(User.MiddleName), user?.MiddleName ?? string.Empty));
                result.Add(new UserProperty(nameof(User.TelephoneNumber), user?.TelephoneNumber ?? string.Empty));
                result.Add(new UserProperty(nameof(User.IsLead), user.IsLead ? "true" : "false"));
                
            }
            catch(Exception ex)
            {
                Logger.Error(ex.Message);

            }
            return result;
           
        }

        public bool IsUserExists(string userLogin)
        {
            bool res = false;
            try
            {
                var user = _dbContext.Users.FirstOrDefault(x => x.Login == userLogin);
                res = user != null ? true : false;
            }
            catch(Exception ex)
            {
                Logger.Error(ex.Message);
            }

            return res;
        }

        public void UpdateUserProperties(IEnumerable<UserProperty> properties, string userLogin)
        {
            try
            {
                var user = _dbContext.Users.FirstOrDefault(x => x.Login == userLogin);
                user.FirstName = properties.FirstOrDefault(x => x.Name == nameof(User.FirstName))?.Value ?? user.FirstName;
                user.MiddleName = properties.FirstOrDefault(x => x.Name == nameof(User.MiddleName))?.Value ?? user.MiddleName;
                user.LastName = properties.FirstOrDefault(x => x.Name == nameof(User.LastName))?.Value ?? user.LastName;
                user.TelephoneNumber = properties.FirstOrDefault(x => x.Name == nameof(User.TelephoneNumber))?.Value ?? user.TelephoneNumber;


                var proplid = properties.FirstOrDefault(x => x.Name == nameof(User.IsLead));
                user.IsLead = proplid == null ? user.IsLead : bool.Parse(proplid.Value);

                _dbContext.Users.Update(user);
                _dbContext.SaveChanges();
            }
            catch(Exception ex)
            {
                Logger.Error(ex.Message);
            }
            
        }

        public IEnumerable<Permission> GetAllPermissions()
        {
            var request = _dbContext.RequestRights.Select(x => new Permission(x.Id.ToString(), x.Name, "RequestRight")).ToList();
            var itrole = _dbContext.ItRoles.Select(x=> new Permission(x.Id.ToString(), x.Name, "ItRole")).ToList();

            var res = request.Concat(itrole);
            
            return res;
        }

        public void AddUserPermissions(string userLogin, IEnumerable<string> rightIds)
        {
           
            try
            {
                var user = _dbContext.Users.FirstOrDefault(x => x.Login == userLogin);
                if (user == null)
                {
                    throw new CustomException($"User с Login:{userLogin} отстутствует");
                }

                foreach (var right in rightIds)
                {
                    
                    var str = right.Split(':', StringSplitOptions.RemoveEmptyEntries);
                    if (str[0].Equals("Request"))
                    {
                        var request = _dbContext.RequestRights.FirstOrDefault(x => x.Id == int.Parse(str[1]));
                        if (request == null)
                        {
                            throw new CustomException("Не найдено право");
                        }

                        var user_request = new UserRequestRight { UserId = userLogin, RightId = int.Parse(str[1]) };
                        _dbContext.UserRequestRights.Add(user_request);
                        
                    }
                    if (str[0].Equals("Role"))
                    {
                        var role = _dbContext.ItRoles.FirstOrDefault(x => x.Id == int.Parse(str[1]));
                        if (role == null)
                        {
                            throw new CustomException("Не найдена роль");
                        }
                        var user_itRole = new UserItrole { UserId = userLogin, RoleId = int.Parse(str[1]) };
                        _dbContext.UserItroles.Add(user_itRole);
                        
                    }
                }

                _dbContext.SaveChanges();

            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
               
            }
               
           
           
        }

        public void RemoveUserPermissions(string userLogin, IEnumerable<string> rightIds)
        {
            try
            {
                var user = _dbContext.Users.FirstOrDefault(x => x.Login == userLogin);
                if (user == null)
                {
                    throw new CustomException($"User с Login:{userLogin} отстутствует");
                }

                foreach (var right in rightIds)
                {

                    var str = right.Split(':', StringSplitOptions.RemoveEmptyEntries);
                    if (str[0].Equals("Request"))
                    {
                        var request = _dbContext.RequestRights.FirstOrDefault(x => x.Id == int.Parse(str[1]));
                        if (request == null)
                        {
                            throw new CustomException("Не найдено право");
                        }

                        var userRequest = _dbContext.UserRequestRights.FirstOrDefault(x=>x.UserId == user.Login && x.RightId == int.Parse(str[1]));
                        _dbContext.UserRequestRights.Remove(userRequest);

                    }
                    if (str[0].Equals("Role"))
                    {
                        var role = _dbContext.ItRoles.FirstOrDefault(x => x.Id == int.Parse(str[1]));
                        if (role == null)
                        {
                            throw new CustomException("Не найдена роль");
                        }
                        var userRole = _dbContext.UserItroles.FirstOrDefault(x => x.UserId == user.Login && x.RoleId == int.Parse(str[1]));
                        _dbContext.UserItroles.Remove(userRole);

                    }
                }

                _dbContext.SaveChanges();

            }
            catch(Exception ex)
            {

            }
        }

        public IEnumerable<string> GetUserPermissions(string userLogin)
        {
            var userPermissions = new List<string>();
            var rightsId = _dbContext.UserRequestRights.Where(x=>x.UserId == userLogin)
                                                      .Select(x=>x.RightId).ToList();

            var itrolesId = _dbContext.UserItroles.Where(x => x.UserId == userLogin)
                                                  .Select(x => x.RoleId).ToList();
            if(rightsId != null)
                foreach (var item in rightsId)
                {
                    var res = _dbContext.RequestRights.FirstOrDefault(x => x.Id == item).Name;
                    if (res != null)
                        userPermissions.Add(res);

                }

            if(itrolesId != null)
                foreach (var item in itrolesId)
                {
                    var res = _dbContext.ItRoles.FirstOrDefault(x => x.Id == item).Name;
                    if (res != null)
                        userPermissions.Add(res);

                }

            return userPermissions;

        }

        public ILogger Logger { get; set; }
        
    }
}
     
