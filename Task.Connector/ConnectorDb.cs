using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection.Metadata;
using System.Text.RegularExpressions;
using Task.Integration.Data.Models;
using Task.Integration.Data.Models.Models;

namespace Task.Connector
{

    public class ConnectorDb : IConnector
    {
        private readonly DbContextOptionsBuilder<ConnectorDbContext> optionsBuilder = new();

        public void StartUp(string connectionString) {
            var reg = new Regex("(?=(?:(?:[^']*'){2})*[^']*$)\\;"); //TODO: Parse the connection string in a better way and switch optionsbuilder to include MSSQL db
            var args = reg.Split(connectionString);
            var argreg = new Regex("(?<=\').*?(?=\')");
            var cs = argreg.Match(args[0]);
            optionsBuilder.UseNpgsql(cs.ToString());
        }

        public void CreateUser(UserToCreate user) {
            try {
                using var context = new ConnectorDbContext(optionsBuilder.Options);

                var newUser = new User() { //this can be simplified
                    Login = user.Login,
                    FirstName =         user.Properties.FirstOrDefault(p => p.Name.Equals(nameof(User.FirstName),       StringComparison.OrdinalIgnoreCase))?.Value ?? "",
                    MiddleName =        user.Properties.FirstOrDefault(p => p.Name.Equals(nameof(User.MiddleName),      StringComparison.OrdinalIgnoreCase))?.Value ?? "",
                    LastName =          user.Properties.FirstOrDefault(p => p.Name.Equals(nameof(User.LastName),        StringComparison.OrdinalIgnoreCase))?.Value ?? "",
                    TelephoneNumber =   user.Properties.FirstOrDefault(p => p.Name.Equals(nameof(User.TelephoneNumber), StringComparison.OrdinalIgnoreCase))?.Value ?? "",
                    IsLead = bool.Parse(user.Properties.FirstOrDefault(p => p.Name.Equals(nameof(User.IsLead),          StringComparison.OrdinalIgnoreCase))?.Value ?? "false")
                };

                var newPassword = new Password() {
                    UserId = user.Login,
                    Password1 = user.HashPassword
                };

                context.Users.Add(newUser);
                context.Passwords.Add(newPassword);
                context.SaveChanges();
            }
            catch (FormatException e) {
                Logger.Error($"Ошибка при создании пользователя - неверный формат свойства:\n {e.Message}");
            }
            catch (Exception e) {
                Logger.Error($"Ошибка при создании пользователя:\n {e.Message}");
            }
        }

        public IEnumerable<Property> GetAllProperties() { //this looks hella ugly
            try {
                using var context = new ConnectorDbContext(optionsBuilder.Options);

                var UserType = context.GetService<IDesignTimeModel>().Model.GetEntityTypes()
                    .Where(type => type.ClrType.Name == nameof(User)).First();

                var UserProperties = UserType!.GetProperties()
                    .Where(prop => prop.Name != nameof(User.Login))
                    .Select(prop => new Property(prop.Name, prop.GetComment() ?? prop.Name));

                var PwdType = context.GetService<IDesignTimeModel>().Model.GetEntityTypes()
                    .Where(type => type.ClrType.Name == nameof(Password)).First();

                var PwdProp = PwdType!.GetProperty(nameof(Password.Password1));

                return UserProperties.Append(new(PwdProp.Name, PwdProp.GetComment() ?? PwdProp.Name));
            }
            catch (Exception e) {
                Logger.Error($"Ошибка при возврате всех свойств пользователей: \n {e.Message}");
                return Enumerable.Empty<Property>();
            }
        }

        public IEnumerable<UserProperty> GetUserProperties(string userLogin) {
            try {
                using var context = new ConnectorDbContext(optionsBuilder.Options);

                var query = from user in context.Users
                            where user.Login == userLogin
                            select new UserProperty[] {
                            new (nameof(user.FirstName),         user.FirstName),
                            new (nameof(user.MiddleName),        user.MiddleName),
                            new (nameof(user.LastName),          user.LastName),
                            new (nameof(user.TelephoneNumber),   user.TelephoneNumber),
                            new (nameof(user.IsLead),            user.IsLead.ToString()),
                        };
                return query.First();
            }
            catch (Exception e) {
                Logger.Error($"Ошибка при возврате свойств пользователя:\n {e.Message}");
                return Enumerable.Empty<UserProperty>();
            }
        }

        public bool IsUserExists(string userLogin) {
            try {
                using var context = new ConnectorDbContext(optionsBuilder.Options);

                return context.Users.Any(u => u.Login == userLogin);
            } catch (Exception e) {
                Logger.Error($"Ошибка при проверке существования пользователя:\n {e.Message}");
                return false;
            }
        }

        public void UpdateUserProperties(IEnumerable<UserProperty> properties, string userLogin) { //invalid operation - user not found
            try {
                using var context = new ConnectorDbContext(optionsBuilder.Options);

                var query = from user in context.Users where user.Login == userLogin select user;
                var UserToUpdate = query.First();

                //this can also be simplified
                UserToUpdate.FirstName =         properties.FirstOrDefault(p => p.Name.Equals(nameof(User.FirstName),       StringComparison.OrdinalIgnoreCase))?.Value ?? UserToUpdate.FirstName;
                UserToUpdate.MiddleName =        properties.FirstOrDefault(p => p.Name.Equals(nameof(User.MiddleName),      StringComparison.OrdinalIgnoreCase))?.Value ?? UserToUpdate.MiddleName;
                UserToUpdate.LastName =          properties.FirstOrDefault(p => p.Name.Equals(nameof(User.LastName),        StringComparison.OrdinalIgnoreCase))?.Value ?? UserToUpdate.LastName;
                UserToUpdate.TelephoneNumber =   properties.FirstOrDefault(p => p.Name.Equals(nameof(User.TelephoneNumber), StringComparison.OrdinalIgnoreCase))?.Value ?? UserToUpdate.TelephoneNumber;
                UserToUpdate.IsLead = bool.Parse(properties.FirstOrDefault(p => p.Name.Equals(nameof(User.IsLead),          StringComparison.OrdinalIgnoreCase))?.Value ?? UserToUpdate.IsLead.ToString());

                context.Users.Update(UserToUpdate);
                context.SaveChanges();
            }
            catch (Exception e) {
                Logger.Error($"Ошибка при обновлении данных пользователя:\n {e.Message}");
            }
        }

        public IEnumerable<Permission> GetAllPermissions() {
            try {

                using var context = new ConnectorDbContext(optionsBuilder.Options);

                var roleQuery = from roles in context.ItRoles
                                select new Permission(roles.Id.ToString(), roles.Name, roles.Name);

                var rightsQuery = from rights in context.RequestRights
                                  select new Permission(rights.Id.ToString(), rights.Name, rights.Name);

                return roleQuery.ToList().Concat(rightsQuery.ToList());
            }
            catch (Exception e) {
                Logger.Error($"Ошибка при возврате всех типов прав:\n {e.Message}");
                return Enumerable.Empty<Permission>();
            }

        }

        public void AddUserPermissions(string userLogin, IEnumerable<string> rightIds) {
            try {
                using var context = new ConnectorDbContext(optionsBuilder.Options);

                var userQuery = from usr in context.Users where usr.Login == userLogin select usr;
                var user = userQuery.FirstOrDefault() ?? null;

                if (user == null) {
                    Logger.Error($"Пользователем с именем {userLogin} не найден в методе AddUserPermissions!");
                    return;
                }

                List<UserItrole> newRoles = new();
                List<UserRequestRight> newRequests = new();

                foreach (string right in rightIds) {
                    var permission = right.Split(':');
                    switch (permission[0]) {
                        case "Role":
                            newRoles.Add(new UserItrole() {
                                UserId = user.Login,
                                RoleId = int.Parse(permission[1])
                            });
                            break;
                        case "Request":
                            newRequests.Add(new UserRequestRight() {
                                UserId = user.Login,
                                RightId = int.Parse(permission[1])
                            });
                            break;
                        default:
                            Logger.Error($"Неверный формат RightId в методе AddUserPermissions: {permission[0]} встречено, ожидалось 'Role' или 'Request'!");
                            break;
                    }
                }

                context.UserItroles.AddRange(newRoles);
                context.UserRequestRights.AddRange(newRequests);
                context.SaveChanges();
            }
            catch (Exception e) {
                Logger.Error($"Ошибка при добавлении прав пользователю:\n {e.Message}");
            }
        }

        public void RemoveUserPermissions(string userLogin, IEnumerable<string> rightIds) {
            try {
                using var context = new ConnectorDbContext(optionsBuilder.Options);

                var userQuery = from usr in context.Users where usr.Login == userLogin select usr;
                var user = userQuery.FirstOrDefault() ?? null;

                if (user == null) {
                    Logger.Error($"Пользователем с именем {userLogin} не найден в методе RemoveUserPermissions!");
                    return;
                }

                List<UserItrole> RolesToRemove = new();
                List<UserRequestRight> RequestsToRemove = new();

                foreach (string right in rightIds) {
                    var permission = right.Split(':');
                    switch (permission[0]) {
                        case "Role":
                            RolesToRemove.Add(new UserItrole() {
                                UserId = user.Login,
                                RoleId = int.Parse(permission[1])
                            });
                            break;
                        case "Request":
                            RequestsToRemove.Add(new UserRequestRight() {
                                UserId = user.Login,
                                RightId = int.Parse(permission[1])
                            });
                            break;
                        default:
                            Logger.Error($"Неверный формат RightId в методе RemoveUserPermissions: {permission[0]} встречено, ожидалось 'Role' или 'Request'!");
                            break;
                    }
                }

                context.UserItroles.RemoveRange(RolesToRemove);
                context.UserRequestRights.RemoveRange(RequestsToRemove);
                context.SaveChanges();
            } catch (Exception e) {
                Logger.Error($"Ошибка при удалении прав пользователя:\n {e.Message}");
            }
        }

        public IEnumerable<string> GetUserPermissions(string userLogin) {
            try {
                using var context = new ConnectorDbContext(optionsBuilder.Options);

                var query = from user in context.UserRequestRights
                            where user.UserId == userLogin
                            join right in context.RequestRights
                                on user.RightId equals right.Id
                            select right.Name;
                return query.ToList();
            }
            catch (Exception e) {
                Logger.Error($"Ошибка при возврате прав пользователя:\n {e.Message}");
                return Enumerable.Empty<string>();
            }
        }

        public ILogger Logger { get; set; } = null!;
    }
}