using Microsoft.EntityFrameworkCore;
using Task.DbModule.Data;
using Task.DbModule.Models;
using Task.Integration.Data.Models;
using Task.Integration.Data.Models.Models;

namespace Task.Connector
{
    public class ConnectorDb : IConnector
    {
	    private BaseContext _context;

        public ConnectorDb()
        {
        }

        public void StartUp(string connectionString)
        {
	        var optionsBuilder = new DbContextOptionsBuilder<BaseContext>();
	        optionsBuilder.UseSqlServer(connectionString);

			try
			{
				_context = new BaseContext(optionsBuilder.Options);

				_context.Database.Migrate();

				if (_context.Database.CanConnect())
				{
					Logger.Debug("Успешное подключение к базе данных.");
				}
				else
				{
					Logger.Error("Не удалось подключиться к базе данных. Проверьте строку подключения.");
				}
			}
			catch (Exception ex)
			{
				Logger.Error($"Ошибка при подключении к базе данных:\n {ex.Message}");
			}
		}

        public void CreateUser(UserToCreate user)
        {
	        if (user == null)
	        {
                Logger.Error("Попытка создать пользователя с отсутствующей ссылкой user");
                throw new ArgumentException("Параметр 'user' не может быть null", nameof(user));
			}

	        using (var transaction = _context.Database.BeginTransaction())
	        {
		        try
		        {
			        var newUser = new User()
			        {
						Login = user.Login,
                        LastName = user.Properties
	                        .FirstOrDefault(p => p.Name.ToLower() == "lastname")?.Value ?? "Фамилия",
                        FirstName = user.Properties
	                        .FirstOrDefault(p => p.Name.ToLower() == "firstname")?.Value ?? "Имя",
                        MiddleName = user.Properties
	                        .FirstOrDefault(p => p.Name.ToLower() == "middlename")?.Value ?? "Отчество",
						TelephoneNumber = user.Properties
							.FirstOrDefault(p => p.Name.ToLower() == "telephonenumber")?.Value ?? "0-000-000-00-00",
						IsLead = bool.TryParse(user.Properties
							.FirstOrDefault(p => p.Name.ToLower() == "islead")?.Value, out var result),
			        };

			        var password = new Password()
			        {
                        UserLogin = user.Login,
				        PasswordHash = user.HashPassword,
			        };

			        _context.Users.Add(newUser);
                    _context.SaveChanges();

                    _context.Passwords.Add(password);
                    _context.SaveChanges();

                    transaction.Commit();
                    Logger.Debug($"Пользователь {user.Login} успешно создан!");
		        }
		        catch (Exception ex)
		        {
                    transaction.Rollback();
                    Logger.Error($"Ошибка при создании пользователя:\n {ex}");
				}
	        }
        }

        public IEnumerable<Property> GetAllProperties()
        {
			var users = _context.Users.Include(u => u.Password).ToList();
			var properties = new List<Property>();

			foreach (var user in users)
			{
				properties.Add(new Property("Login", user.Login));
				properties.Add(new Property("LastName", user.LastName));
				properties.Add(new Property("FirstName", user.FirstName));
				properties.Add(new Property("MiddleName", user.MiddleName));
				properties.Add(new Property("TelephoneNumber", user.TelephoneNumber));
				properties.Add(new Property("IsLead", user.IsLead.ToString()));

				if (user.Password != null)
					properties.Add(new Property("PasswordHash", user.Password.PasswordHash));
			}

			return properties;
		}

        public IEnumerable<UserProperty> GetUserProperties(string userLogin)
        {
	        if (!IsUserExists(userLogin))
	        {
                Logger.Warn($"Пользователь с логином {userLogin} не найден!");
		        return new List<UserProperty>();
	        }

	        var user = _context.Users.First(u => u.Login == userLogin);

	        var userProps = new List<UserProperty>
	        {
		        new UserProperty("LastName", user.LastName),
		        new UserProperty("FirstName", user.FirstName),
		        new UserProperty("MiddleName", user.MiddleName),
		        new UserProperty("TelephoneNumber", user.TelephoneNumber),
		        new UserProperty("IsLead", user.IsLead.ToString()),
	        };

			Logger.Debug($"Свойства пользователя с логином {user.Login} были успешно получены!");

			return userProps;
        }

        public bool IsUserExists(string userLogin)
        {
			return _context.Users.Any(u => u.Login == userLogin);
		}

        public void UpdateUserProperties(IEnumerable<UserProperty> properties, string userLogin)
        {
			if (!IsUserExists(userLogin))
			{
				Logger.Warn($"Пользователь с логином {userLogin} не найден!");
				throw new ArgumentException($"Пользователь с логином {userLogin} не найден.", nameof(userLogin));
			}

			var user = _context.Users.Include(u => u.Password).First(u => u.Login == userLogin);

			using (var transaction = _context.Database.BeginTransaction())
			{
				try
				{
					foreach (var property in properties)
					{
						switch (property.Name)
						{
							case "LastName":
								user.LastName = property.Value;
								break;
							case "FirstName":
								user.FirstName = property.Value;
								break;
							case "MiddleName":
								user.MiddleName = property.Value;
								break;
							case "TelephoneNumber":
								user.TelephoneNumber = property.Value;
								break;
							case "IsLead":
								if (bool.TryParse(property.Value, out bool isLead))
								{
									user.IsLead = isLead;
								}
								else
								{
									Logger.Warn($"Некорректное значение для свойства IsLead: {property.Value}");
								}
								break;
							case "PasswordHash":
							case "HashPassword":
								if (user.Password == null)
								{
									user.Password = new Password
									{
										UserLogin = userLogin,
										PasswordHash = property.Value
									};

									_context.Passwords.Add(user.Password);
								}
								else
								{
									user.Password.PasswordHash = property.Value;
								}
								break;
							default:
								Logger.Warn($"Свойство {property.Name} не распознано и не может быть обновлено!");
								break;
						}
					}

					_context.SaveChanges();
					transaction.Commit();
					Logger.Debug($"Свойства пользователя с логином {userLogin} успешно обновлены.");
				}
				catch (Exception ex)
				{
					transaction.Rollback();
					Logger.Error($"Ошибка при обновлении свойств пользователя с логином {userLogin}:\n {ex}");
				}
			}
		}

        public IEnumerable<Permission> GetAllPermissions()
        {
	        return _context.RequestRights.AsNoTracking().Select(request => new Permission(request.Id.ToString(), request.Name, ""));
        }

        public void AddUserPermissions(string userLogin, IEnumerable<string> rightIds)
        {
			if (!IsUserExists(userLogin))
			{
				Logger.Warn($"Пользователь с логином {userLogin} не найден!");
				throw new ArgumentException($"Пользователь с логином {userLogin} не найден.", nameof(userLogin));
			}

			var requestRights =_context.RequestRights.AsNoTracking().Select(re => re.Id.ToString());

			var userRequestRights = new List<UserRequestRight>();

			foreach (var rightId in rightIds)
			{
				if (!requestRights.Contains(rightId))
				{
					Logger.Warn($"Право под номером {rightId} не найдено!");
				}
				else
				{
					if (UInt32.TryParse(rightId, out var result))
					{
						userRequestRights.Add(new UserRequestRight()
						{
							UserLogin = userLogin,
							RequestRightId = result,
						});
					}
				}
			}

			try
			{
				_context.UserRequestRights.AddRange(userRequestRights);
				_context.SaveChanges();
				Logger.Debug($"Выбранные права были успешно добавлены пользователю с логином {userLogin}!");
			}
			catch (Exception ex)
			{
				Logger.Error($"Ошибка при обновлении прав пользователя:\n {ex}");
			}
        }

        public void RemoveUserPermissions(string userLogin, IEnumerable<string> rightIds)
        {
			if (!IsUserExists(userLogin))
			{
				Logger.Warn($"Пользователь с логином {userLogin} не найден!");
				throw new ArgumentException($"Пользователь с логином {userLogin} не найден.", nameof(userLogin));
			}

			var requestRights = _context.RequestRights.AsNoTracking().Select(re => re.Id.ToString());

			var userRequestRights = new List<UserRequestRight>();

			foreach (var rightId in rightIds)
			{
				if (!requestRights.Contains(rightId))
				{
					Logger.Warn($"Право под номером {rightId} не найдено!");
				}
				else
				{
					if (UInt32.TryParse(rightId, out var result))
					{
						userRequestRights.Add(new UserRequestRight()
						{
							UserLogin = userLogin,
							RequestRightId = result,
						});
					}
				}
			}

			try
			{
				_context.UserRequestRights.RemoveRange(userRequestRights);
				_context.SaveChanges();
				Logger.Debug($"Выбранные права пользователя с логином {userLogin} были успешно удалены!");
			}
			catch (Exception ex)
			{
				Logger.Error($"Ошибка при удалении прав пользователя:\n {ex}");
			}
		}

        public IEnumerable<string> GetUserPermissions(string userLogin)
        {
			if (!IsUserExists(userLogin))
			{
				Logger.Warn($"Пользователь с логином {userLogin} не найден!");
				throw new ArgumentException($"Пользователь с логином {userLogin} не найден.", nameof(userLogin));
			}

			return _context.UserRequestRights.Where(urr => urr.UserLogin == userLogin)
				.Include(urr => urr.RequestRight).Select(urr => urr.RequestRight.Name).ToList();
        }

        public ILogger Logger { get; set; }
    }
}