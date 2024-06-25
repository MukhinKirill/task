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
        public void StartUp(string connectionString)
        {
            var reg = new Regex("(?=(?:(?:[^']*'){2})*[^']*$)\\;"); //TODO: Parse the connection string in a better way and switch optionsbuilder to include MSSQL db
            var args = reg.Split(connectionString);
            var argreg = new Regex("(?<=\').*?(?=\')");
            var cs = argreg.Match(args[0]);
            optionsBuilder.UseNpgsql(cs.ToString());
        }

        public void CreateUser(UserToCreate user)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<Property> GetAllProperties() //TODO: Include password as property
        {
            using (var context = new ConnectorDbContext(optionsBuilder.Options)) { // would ideally want some way to get descriptions from db comments but idk how rn...
                var UserType = context.GetService<IDesignTimeModel>().Model.GetEntityTypes().Where(type => type.ClrType.Name == nameof(User)).First();
                var UserProperties = UserType!.GetProperties()
                    .Where(prop => prop.Name != nameof(User.Login))
                    .Select(prop => new Property(prop.Name, prop.GetComment() ?? prop.Name));                

                foreach (var property in UserProperties) {
                    Logger.Debug(" --- " + property.Name + " : " + property.Description);
                }

                return UserProperties;
            }
            
        }

        public IEnumerable<UserProperty> GetUserProperties(string userLogin)
        {
            using (var context = new ConnectorDbContext(optionsBuilder.Options)) {
                var query = from user in context.Users
                            where user.Login == userLogin
                            select new UserProperty[] {
                                new(nameof(user.FirstName),         user.FirstName),
                                new(nameof(user.MiddleName),        user.MiddleName),
                                new(nameof(user.LastName),          user.LastName),
                                new(nameof(user.TelephoneNumber),   user.TelephoneNumber),
                                new(nameof(user.IsLead),            user.IsLead.ToString()),
                            };
                return query.First();
            }
        }

        public bool IsUserExists(string userLogin)
        {
            using (var context = new ConnectorDbContext(optionsBuilder.Options)) {
                return context.Users.Any(u => u.Login == userLogin);
            }
        }

        public void UpdateUserProperties(IEnumerable<UserProperty> properties, string userLogin)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<Permission> GetAllPermissions()
        {
            throw new NotImplementedException();
        }

        public void AddUserPermissions(string userLogin, IEnumerable<string> rightIds)
        {
            throw new NotImplementedException();
        }

        public void RemoveUserPermissions(string userLogin, IEnumerable<string> rightIds)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<string> GetUserPermissions(string userLogin)
        {
            using (var context = new ConnectorDbContext(optionsBuilder.Options)) {
                var query = from user in context.UserRequestRights
                            where user.UserId == userLogin
                            join right in context.RequestRights
                                on user.RightId equals right.Id
                            select right.Name;
                return query.ToList();
            }
        }

        public ILogger Logger { get; set; } = null!;
    }
}