using Task.Integration.Data.Models;
using Task.Integration.Data.Models.Models;

using Task.Integration.Data.DbCommon;
using Task.Integration.Data.DbCommon.DbModels;

using Task.Connector.Extensions;

namespace Task.Connector
{
    public class ConnectorDb : IConnector
    {
        private DbContextFactory _dbContextFactory;
        private string _provider;

        public ILogger Logger { get; set; }

        public void StartUp(string connectionString)
        {
            _dbContextFactory = new DbContextFactory(connectionString.GetDbConnectionString());
            _provider = connectionString.GetDbProvider();
        }

        public void CreateUser(UserToCreate user)
        {
            var userData = user.SetOrDefaultProperties();
            var userCredentials = new Sequrity()
            {
                UserId = user.Login,
                Password = user.HashPassword
            };
            try
            {
                using (var context = _dbContextFactory.GetContext(_provider))
                {
                    context.Users.Add(userData);
                    context.Passwords.Add(userCredentials);
                    context.SaveChanges();
                }

                Logger.Debug($"The user \"{user.Login}\" has been successfully created!");
            }
            catch(Exception e)
            {
                Logger.Error($"Failed to create user \"{user.Login}\"!\n" +
                             $"The following error occurred: {e.Message}\n" +
                             $"The inner exception: {e.InnerException?.Message}");
            }
        }

        public IEnumerable<Property> GetAllProperties()
        {
            Logger.Debug($"A list of user properties has been requested.");
            return new Property[]
            {
                new Property("First name", "Имя"),
                new Property("Middle name", "Отчество"),
                new Property("Last name", "Фамилия"),
                new Property("Telephone number", "Номер телефона"),
                new Property("Is lead", "Является ли лидером"),
                new Property("Password", "Пароль")
            };
        }

        public IEnumerable<UserProperty> GetUserProperties(string userLogin)
        {
            User? user = null;
            var properties = Array.Empty<UserProperty>();
            try
            {
                using (var context = _dbContextFactory.GetContext(_provider))
                {
                    user = context.Users.Find(userLogin);
                }
            }
            catch(Exception e)
            {
                Logger.Error($"It was not possible to get the properties of the user \"{userLogin}\" from the database!\n" +
                             $"The following error occurred: {e.Message}\n" +
                             $"The inner exception: {e.InnerException?.Message}");
            }

            if (user == null)
            {
                Logger.Warn($"The properties of the user \"{userLogin}\" were requested, but it was not found in the database!");
            }
            else
            {
                properties = new UserProperty[]
                {
                    new UserProperty("First name", user.FirstName),
                    new UserProperty("Middle name", user.MiddleName),
                    new UserProperty("Last name", user.LastName),
                    new UserProperty("Telephone number", user.TelephoneNumber),
                    new UserProperty("Is lead", user.IsLead.ToString()),
                };

                Logger.Debug($"The properties of the \"{userLogin}\" user have been requested.");
            }

            return properties;
        }

        public bool IsUserExists(string userLogin)
        {
            var status = false;

            try
            {
                using (var context = _dbContextFactory.GetContext(_provider))
                {
                    if (context.Users.Find(userLogin) != null)
                    {
                        Logger.Debug($"A user with the username \"{userLogin}\" is present in the database.");
                        status = true;
                    }
                    else
                    {
                        Logger.Warn($"A user with the username {userLogin} is not present in the database.");
                        status = false;
                    }
                }
            }
            catch(Exception e)
            {
                Logger.Error($"It was not possible to get information from the database, whether there is a user \"{userLogin}\" in it!\n" +
                             $"The following error occurred: {e.Message}\n" +
                             $"The inner exception: {e.InnerException?.Message}");
            }

            return status;
        }

        public void UpdateUserProperties(IEnumerable<UserProperty> properties, string userLogin)
        {
            try
            {
                using (var context = _dbContextFactory.GetContext(_provider))
                {
                    var user = context.Users.Find(userLogin);
                    if (user == null)
                    {
                        Logger.Warn($"A request to change the properties of the user \"{userLogin}\", but it was not found in the database!");
                    }
                    else
                    {
                        foreach (var property in properties)
                        {
                            if (property.Name == "First name") user.FirstName = property.Value;
                            else if (property.Name == "Middle name") user.MiddleName = property.Value;
                            else if (property.Name == "Last name") user.LastName = property.Value;
                            else if (property.Name == "Telephone number") user.TelephoneNumber = property.Value;
                            else if (property.Name == "Is lead") user.IsLead = Convert.ToBoolean(property.Value);
                        }
                        context.Users.Update(user);
                        context.SaveChanges();

                        Logger.Warn($"The properties of the user \"{userLogin}\" have been changed.");
                    }

                }
            }
            catch(Exception e)
            {
                Logger.Error($"Changing the properties of the user \"{userLogin}\" failed!\n" +
                             $"The following error occurred: {e.Message}\n" +
                             $"The inner exception: {e.InnerException?.Message}");
            }
        }

        public IEnumerable<Permission> GetAllPermissions()
        {
            var permissions = new List<Permission>();
            try
            {
                using (var context = _dbContextFactory.GetContext(_provider))
                {
                    permissions.AddRange(context.ITRoles.Select(itRole =>
                        new Permission(itRole.Id.ToString(), itRole.Name, "The role of the employee.")));
                    permissions.AddRange(context.RequestRights.Select(requestRight =>
                        new Permission(requestRight.Id.ToString(), requestRight.Name, "Access permissions.")));

                    Logger.Debug("Requesting a list of all rights in the database.");
                }
            }
            catch(Exception e)
            {
                Logger.Error($"Couldn't get a list of all rights in the database!\n" +
                             $"The following error occurred: {e.Message}\n" +
                             $"The inner exception: {e.InnerException?.Message}");
            }

            return permissions;
        }

        public void AddUserPermissions(string userLogin, IEnumerable<string> rightIds)
        {
            try
            {
                using (var context = _dbContextFactory.GetContext(_provider))
                {
                    if (context.Users.Find(userLogin) == null)
                    {
                        Logger.Warn($"A request was received to add rights to the user \"{userLogin}\", but it was not found in the database!");
                    }
                    else
                    {
                        foreach (var rightInfo in rightIds)
                        {
                            var right = rightInfo.Split(':');
                            if (right[0] == "Role")
                            {
                                context.UserITRoles.Add(new UserITRole()
                                {
                                    UserId = userLogin,
                                    RoleId = int.Parse(right[1])
                                });
                            }
                            else
                            {
                                context.UserRequestRights.Add(new UserRequestRight()
                                {
                                    UserId = userLogin,
                                    RightId = int.Parse(right[1])
                                });
                            }
                        }
                        context.SaveChanges();

                        Logger.Debug($"Rights have been added to the user \"{userLogin}\".");
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Error($"Failed to add rights to the user \"{userLogin}\"!\n" +
                             $"The following error occurred: {e.Message}\n" +
                             $"The inner exception: {e.InnerException?.Message}");
            }
        }

        public void RemoveUserPermissions(string userLogin, IEnumerable<string> rightIds)
        {
            try
            {
                using (var context = _dbContextFactory.GetContext(_provider))
                {
                    if (context.Users.Find(userLogin) == null)
                    {
                        Logger.Warn($"A request was received to remove rights to the user \"{userLogin}\", but it was not found in the database!");
                    }
                    else
                    {
                        foreach (var rightInfo in rightIds)
                        {
                            var right = rightInfo.Split(':');
                            if (right[0] == "Role")
                            {
                                context.UserITRoles.Remove(new UserITRole()
                                {
                                    UserId = userLogin,
                                    RoleId = int.Parse(right[1])
                                });
                            }
                            else
                            {
                                context.UserRequestRights.Remove(new UserRequestRight()
                                {
                                    UserId = userLogin,
                                    RightId = int.Parse(right[1])
                                });
                            }
                        }
                        context.SaveChanges();

                        Logger.Debug($"Rights have been removed from the user \"{userLogin}\".");
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Error($"Failed to remove rights to the user \"{userLogin}\"!\n" +
                             $"The following error occurred: {e.Message}\n" +
                             $"The inner exception: {e.InnerException?.Message}");
            }
        }

        public IEnumerable<string> GetUserPermissions(string userLogin)
        {
            throw new NotImplementedException();
        }
    }
}