using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.RegularExpressions;
using Task.Integration.Data.Models;
using Task.Integration.Data.Models.Models;

namespace Task.Connector
{

    public class ConnectorDb : IConnector
    {
        private readonly DbContextOptionsBuilder<ConnectorDbContext> optionsBuilder = new DbContextOptionsBuilder<ConnectorDbContext>();
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

        public IEnumerable<Property> GetAllProperties()
        {
            throw new NotImplementedException();
        }

        public IEnumerable<UserProperty> GetUserProperties(string userLogin)
        {
            throw new NotImplementedException();
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
                var query = from usr in context.UserRequestRights
                            where usr.UserId == userLogin
                            join right in context.RequestRights
                                on usr.RightId equals right.Id
                            select right.Name;
                return query.ToList();
            }
        }

        public ILogger Logger { get; set; }
    }
}