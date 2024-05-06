using Dapper;
using Npgsql;
using System.Data;
using Task.Connector.Models;
using Task.Integration.Data.Models;
using Task.Integration.Data.Models.Models;
using System.ComponentModel.DataAnnotations;
using System.Data.SqlClient;
using System.Data.Common;
using Microsoft.EntityFrameworkCore;


namespace Task.Connector
{
    public class ConnectorDb : IConnector
    {
        public ILogger Logger { get; set; }

        private DbConnectionStringBuilder config;

        //private ConnectorDbContext _dbContext;

		public void StartUp(string connectionString)
        {
            config = new DbConnectionStringBuilder();
            config.ConnectionString = connectionString;

            //_dbContext = new ConnectorDbContext((string)config["ConnectionString"], (string)config["SchemaName"]);
        }

		private ConnectorDbContext NewDbContext() 
		{
			return new ConnectorDbContext((string) config["ConnectionString"], (string)config["SchemaName"]);
		}

		public void CreateUser(UserToCreate user)
		{
			Logger.Debug($"ConnectorDb.CreateUser {user.Login}");
            var userobj = new User()
            {
                Login = user.Login
            };

            var userpro = userobj.GetType().GetProperties();

            Array.ForEach(userpro, a =>
            {
                var uprop = user.Properties.FirstOrDefault(f => f.Name == a.Name);
                if (uprop != null)
                {
                    a.SetValue(userobj, Convert.ChangeType(uprop.Value, a.PropertyType));
                }
            });

            using (var db = NewDbContext())
            {
				db.Users.Add(userobj);
				db.Passwords.Add(new Password(user.Login, user.HashPassword));
				db.SaveChanges();
			}
		}

		public IEnumerable<Property> GetAllProperties()
		{
			Logger.Debug($"ConnectorDb.GetAllProperties");
			var properies = new List<Property>();

			var userProps = typeof(User).GetProperties();
			foreach (var prop in userProps) {
				var dispAttr = prop.GetCustomAttributes(typeof(DisplayAttribute), false).OfType<DisplayAttribute>().FirstOrDefault();
				if (dispAttr != null)
					properies.Add(new Property(prop.Name, dispAttr?.Description));
			}

			var passProps = typeof(Password).GetProperties();
			foreach(var prop in passProps) {
				var dispAttr = prop?.GetCustomAttributes(typeof(DisplayAttribute), false).OfType<DisplayAttribute>().FirstOrDefault();
				if (dispAttr != null)
					properies.Add(new Property(prop.Name, dispAttr?.Description));
			}

			return properies;
		}

		public IEnumerable<UserProperty> GetUserProperties(string userLogin)
		{
			Logger.Debug($"ConnectorDb.GetUserProperties {userLogin}");

			User? user = null;
			using (var db = NewDbContext())
			{
				user = db.Users.SingleOrDefault(w => w.Login == userLogin);
			}
			
			var userProps = new List<UserProperty>();
			if (user == null) { return userProps; }
			foreach (var prop in user.GetType().GetProperties())
			{
				var dispAttr = prop?.GetCustomAttributes(typeof(DisplayAttribute), false).OfType<DisplayAttribute>().FirstOrDefault();
				if (dispAttr != null)
					userProps.Add(new UserProperty(prop.Name, prop.GetValue(user).ToString())); 
			}

			return userProps;
		}

        public bool IsUserExists(string userLogin)
        {
			Logger.Debug($"ConnectorDb.IsUserExists {userLogin}");
			bool result = false;
			using (var db = NewDbContext())
			{
				result = db.Users.Any(w => w.Login == userLogin);
			}
			
			return result;
		}

        public void UpdateUserProperties(IEnumerable<UserProperty> properties, string userLogin)
        {
			Logger.Debug($"ConnectorDb.UpdateUserProperties {userLogin}");

			User? user = null;
			using (var db = NewDbContext())
			{
				user = db.Users.SingleOrDefault(w => w.Login == userLogin);
				if (user == null) { return; }

				var uprops = user.GetType().GetProperties();
				foreach (var prop in properties)
				{
					var updateprop = uprops.SingleOrDefault(s => s.Name == prop.Name);
					updateprop.SetValue(user, Convert.ChangeType(prop.Value, updateprop.PropertyType));
				}

				db.SaveChanges();
			}
		}

		public IEnumerable<Permission> GetAllPermissions()
		{
			Logger.Debug($"ConnectorDb.GetAllPermissions");
			var permissions = new List<Permission>();

			List<RequestRight> requestRights;
			List<ItRole> roles;
			using (var db = NewDbContext())
			{
				requestRights = db.RequestRights.ToList();
				roles = db.ItRoles.ToList();
			}

			foreach (var reqRight in requestRights)
			{
				permissions.Add(new Permission(reqRight.Id.ToString(), reqRight.Name, null));
			}
			foreach (var role in roles)
			{
				permissions.Add(new Permission(role.Id.ToString(), role.Name, null));
			}
			
			return permissions;
		}

        public void AddUserPermissions(string userLogin, IEnumerable<string> rightIds)
        {
			Logger.Debug($"ConnectorDb.AddUserPermissions {userLogin}");

			using (var db = NewDbContext())
			{
				foreach (var strRightId in rightIds)
				{
					var splitParam = strRightId.Split(':');

					if (splitParam[0] == "Role")
					{
						var roleId = Convert.ToInt32(splitParam[1]);
						db.UserITRoles.Add(new UserITRole() { UserId = userLogin, RoleId = roleId });
					}
					if (splitParam[0] == "Request")
					{
						var rightId = Convert.ToInt32(splitParam[1]);
						db.UserRequestRights.Add(new UserRequestRight() { UserId = userLogin, RightId = rightId });
					}
				}
				db.SaveChanges();
			}
		}

		public void RemoveUserPermissions(string userLogin, IEnumerable<string> rightIds)
		{
			//_dbContext.UserRequestRights.Where(w => w.UserId == userLogin).Load();
			Logger.Debug($"ConnectorDb.RemoveUserPermissions {userLogin}");

			using (var db = NewDbContext())
			{
				foreach (var strRightId in rightIds)
				{
					var splitParam = strRightId.Split(':');
					if (splitParam[0] == "Role")
					{
						var roleId = Convert.ToInt32(splitParam[1]);
						var userItRole = db.UserITRoles.SingleOrDefault(s => s.UserId == userLogin && s.RoleId == roleId);
						if (userItRole != null)
						{
							db.UserITRoles.Remove(userItRole);
						}
					}
					if (splitParam[0] == "Request")
					{
						var rightId = Convert.ToInt32(splitParam[1]);
						var userRequestRight = db.UserRequestRights.Where(s => s.UserId == userLogin && s.RightId == rightId).SingleOrDefault();
						if (userRequestRight != null)
						{
							db.UserRequestRights.Remove(userRequestRight);
						}
					}
				}
				db.SaveChanges();
			}
		}

        public IEnumerable<string> GetUserPermissions(string userLogin)
        {
			Logger.Debug($"ConnectorDb.GetUserPermissions {userLogin}");
			IEnumerable<string> userPermissions;
			using (var db = NewDbContext())
			{
				var up = from ur in db.UserRequestRights.AsNoTracking()
							   join r in db.RequestRights.AsNoTracking() on ur.RightId equals r.Id
							   select r.Name;
				userPermissions = up.ToList();
			 }
			return userPermissions;
		}

    }
}