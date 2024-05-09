using System.Collections;
using Task.Connector.Domain.Models;
using Task.Connector.Infrastructure.Context;
using Task.Connector.Infrastructure.DataModels;
using Task.Connector.Infrastructure.Repository;
using Task.Connector.Infrastructure.Repository.Interfaces;
using Task.Integration.Data.Models;
using Task.Integration.Data.Models.Models;

namespace Task.Connector
{
    public class ConnectorDb : IConnector
    {
        private AvanpostContext _context;
        private IUserRepository _userRepository;
        private IPermissionRepository _permissionRepository;
        public ILogger Logger { get; set; }
        public void StartUp(string connectionString)
        {
            var contextFactory = new AvanpostContextFactory(connectionString, Logger);
            _context = contextFactory.GetContext("POSTGRE");
            _userRepository = new UserRepository(_context, Logger);
            _permissionRepository = new PermissionRepository(_context, Logger);
        }

        public void CreateUser(UserToCreate user)
        {
            try
            {
                var userDataModel = new UserDataModel(user);
                
                var isLeadString = userDataModel.IsLead ? "lead" : "not lead";
                Logger.Debug($"Creating {isLeadString}. Properties: " +
                             $"login - '{userDataModel.Login}'; " +
                             $"firstName - '{userDataModel.FirstName}'; " +
                             $"middleName - '{userDataModel.MiddleName}'; " +
                             $"lastName - '{userDataModel.LastName}'; " +
                             $"telephoneNumber - '{userDataModel.TelephoneNumber}'");

                _userRepository.Create(userDataModel);

                Logger.Debug($"Created user {userDataModel.Login}");
            }
            catch (Exception e)
            {
                Logger.Error($"User creating error {e.Message}");
                throw;
            }
        }
        
        public bool IsUserExists(string userLogin)
        {
            try
            {
                var isExists = _userRepository.IsExists(userLogin);
                Logger.Debug($"Check user {userLogin}. Result {isExists}");
                return isExists;
            }
            catch (Exception e)
            {
                Logger.Error($"User check error: {e.Message}");
                throw;
            }
        }

        public IEnumerable<Property> GetAllProperties()
        {
            try
            {
                Logger.Debug("Get All Properties");
                var properties = UserDataModel.GetProperties();
                return properties;
            }
            catch (Exception e)
            {
                Logger.Error($"User check error: {e.Message}");
                throw;
            }
        }

        public IEnumerable<UserProperty> GetUserProperties(string userLogin)
        {
            try
            {
                Logger.Debug("Get User Properties");

                var user = _userRepository.GetUserModelByLogin(userLogin);
                if (user == null) throw new Exception("User not found");
                
                var properties = user.GetUserProperties();
                return properties;
            }
            catch (Exception e)
            {
                Logger.Error($"Get User Properties error: {e.Message}");
                throw;
            }
        }

        public void UpdateUserProperties(IEnumerable<UserProperty> properties, string userLogin)
        {
            try
            {
                Logger.Debug("Get User Properties");

                var user = _userRepository.GetUserModelByLogin(userLogin);
                if (user == null) throw new Exception("User not found");

                foreach (var property in properties)
                {
                    var test = typeof(UserDataModel).GetProperty(property.Name);
                    if (test == null) throw new Exception("User Property not found");
                    
                    test.SetValue(user, property.Value);
                }
                
                _userRepository.Update(user);
                
            }
            catch (Exception e)
            {
                Logger.Error($"Get User Properties error: {e.Message}");
                throw;
            }
        }

        public IEnumerable<Permission> GetAllPermissions()
        {
            try
            {
                Logger.Debug("Get All Permissions");
                return _permissionRepository.GetAllPermissions();
            }
            catch (Exception e)
            {
                Logger.Error($"Get All Permissions error: {e.Message}");
                throw;
            }
        }

        public void AddUserPermissions(string userLogin, IEnumerable<string> rightIds)
        {
            try
            {
                Logger.Debug("Starting adding User Permissions");

                var isExists = _userRepository.IsExists(userLogin);
                if (!isExists) throw new Exception("User not found");
                
                var roles = new List<UserItRole>();
                var requests = new List<UserRequestRight>();
                
                foreach (var rightId in rightIds)
                {
                    var values = rightId.Split(':', 2);
            
                    switch (values[0])
                    {
                        case "Role":
                            roles.Add(new UserItRole()
                            {
                                UserId = userLogin,
                                RoleId = int.Parse(values[1])
                            });
                            break;

                        case "Request":
                            requests.Add(new UserRequestRight()
                            {
                                UserId = userLogin,
                                RightId = int.Parse(values[1])
                            });
                            break;
                    }
                    
                    _permissionRepository.AddRequestPermissions(requests);
                    _permissionRepository.AddRolePermissions(roles);
                    
                    Logger.Debug("End adding User Permissions");
                }
            }
            catch (Exception e)
            {
                Logger.Error($"Adding User Permissions error: {e.Message}");
                throw;
            }
        }

        public void RemoveUserPermissions(string userLogin, IEnumerable<string> rightIds)
        {
            try
            {
                Logger.Debug("Starting removing User Permissions");

                var isExists = _userRepository.IsExists(userLogin);
                if (!isExists) throw new Exception("User not found");
                
                var roles = new List<UserItRole>();
                var requests = new List<UserRequestRight>();
                
                foreach (var rightId in rightIds)
                {
                    var values = rightId.Split(':', 2);
            
                    switch (values[0])
                    {
                        case "Role":
                            roles.Add(new UserItRole()
                            {
                                UserId = userLogin,
                                RoleId = int.Parse(values[1])
                            });
                            break;

                        case "Request":
                            requests.Add(new UserRequestRight()
                            {
                                UserId = userLogin,
                                RightId = int.Parse(values[1])
                            });
                            break;
                    }
                    
                    _permissionRepository.RemoveRequestPermissions(requests);
                    _permissionRepository.RemoveRolePermissions(roles);
                    
                    Logger.Debug("End removing User Permissions");
                }
            }
            catch (Exception e)
            {
                Logger.Error($"Removing User Permissions error: {e.Message}");
                throw;
            }
        }

        public IEnumerable<string> GetUserPermissions(string userLogin)
        {
            try
            {
                Logger.Debug("Get All User Permissions");
                var permissions = _permissionRepository.GetUserPermissions(userLogin);
                
                return permissions.Select(p => $"{p.Name}:{p.Id}");
            }
            catch (Exception e)
            {
                Logger.Error($"Get All User Permissions error: {e.Message}");
                throw;
            }
        }
    }
}