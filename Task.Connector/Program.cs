using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Task.Integration.Data.Models.Models;

namespace Task.Connector
{
    class Program
    {
        static void Main(string[] args)
        {
            var connection = new SqlConnection();
            var logger = new ConsoleLogger();
            var connector = new Connector(connection, logger);

            connector.StartUp("Server=127.0.0.1;Port=5432;Database=testDb;Username=testUser;Password=12345678;\" -p \"POSTGRE");



            var user = new UserToCreate
            {
                Login = "user1",
                Name = "John Doe"
            };

            connector.CreateUser(user);
            var userExists = connector.IsUserExists("user1");

            var properties = connector.GetAllProperties();
            var userProperties = connector.GetUserProperties("user1");

            connector.UpdateUserProperties(userProperties, "user1");
        }

    }
}
