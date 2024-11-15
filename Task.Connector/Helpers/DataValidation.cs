using Task.Connector.Contexts;
using Task.Connector.Models;

namespace Task.Connector.Helpers
{
    public class DataValidation
    {
        public bool UserExists(string userLogin, out User? user)
        {
            using (ConnectorDbContext db = new ConnectorDbContext())
            {
                user = db.Users.FirstOrDefault(u => u.Login == userLogin);
                return user == null;
            } 
        }
    }
}
