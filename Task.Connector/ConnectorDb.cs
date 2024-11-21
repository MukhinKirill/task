using System.Collections;
using Microsoft.EntityFrameworkCore;
using System.Reflection;
using Task.DbModule.Data;
using Task.DbModule.Models;
using Task.Integration.Data.Models;
using Task.Integration.Data.Models.Models;
using System.Linq;

namespace Task.Connector
{
	public class ConnectorDb : IConnector
	{
		private BaseContext _context;
		public ConnectorDb()
		{
		}

		public ILogger Logger { get; set; }

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
					Logger?.Debug("Успешное подключение к базе данных.");
				}
				else
				{
					Logger?.Error("Не удалось подключиться к базе данных. Проверьте строку подключения.");
				}
			}
			catch (Exception ex)
			{
				Logger?.Error($"Ошибка при подключении к базе данных:\n {ex.Message}");
				throw;
			}
		}

		public void CreateUser(UserToCreate user)
		{
			if (user == null)
			{
				Logger?.Error("Попытка создать пользователя с отсутствующей ссылкой user");
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
					//Logger?.Debug($"Пользователь {user.Login} успешно создан!");
				}
				catch (Exception ex)
				{
					transaction.Rollback();
					//Logger?.Error($"Ошибка при создании пользователя:\n {ex}");
					throw;
				}
			}
		}

		public IEnumerable<Property> GetAllProperties()
		{
			var allProperties = new List<Property>();
			var user = new User()
			{
				Login = "login",
				FirstName = "firstName",
				LastName = "lastName",
				MiddleName = "middleName",
				TelephoneNumber = "0-000-000-00-00",
				IsLead = false,
			};

			var userProperties = typeof(User).GetProperties();

			foreach (var property in user.GetType().GetProperties())
			{
				string propertyName = property.Name;
				string? propertyValue = property.GetValue(user)?.ToString();
				if (!String.IsNullOrEmpty(propertyName) && !String.IsNullOrEmpty(propertyValue) && propertyName != "Login")
					allProperties.Add(new Property(propertyName, propertyValue));
			}

			allProperties.Add(new Property("Password", "Password"));

			return allProperties;
		}

		public IEnumerable<UserProperty> GetUserProperties(string userLogin)
		{
			if (!IsUserExists(userLogin))
			{
				Logger?.Warn($"Пользователь с логином {userLogin} не найден!");
				return new List<UserProperty>();
			}

			var user = _context.Users.AsNoTracking().First(u => u.Login == userLogin);

			var userProps = new List<UserProperty>
			{
				new UserProperty("LastName", user.LastName),
				new UserProperty("FirstName", user.FirstName),
				new UserProperty("MiddleName", user.MiddleName),
				new UserProperty("TelephoneNumber", user.TelephoneNumber),
				new UserProperty("IsLead", user.IsLead.ToString()),
			};

			//Logger?.Debug($"Свойства пользователя с логином {user.Login} были успешно получены!");

			return userProps;
		}

		public bool IsUserExists(string userLogin)
		{
			return _context.Users.AsNoTracking().Any(u => u.Login == userLogin);
		}

		public void UpdateUserProperties(IEnumerable<UserProperty> properties, string userLogin)
		{
			if (!IsUserExists(userLogin))
			{
				//Logger?.Warn($"Пользователь с логином {userLogin} не найден!");
				throw new ArgumentException($"Пользователь с логином {userLogin} не найден.", nameof(userLogin));
			}

			var user = _context.Users.First(u => u.Login == userLogin);
			var userPassword = _context.Passwords.FirstOrDefault(p => p.UserLogin == userLogin);

			using (var transaction = _context.Database.BeginTransaction())
			{
				try
				{
					foreach (var property in properties)
					{
						switch (property.Name.ToLower())
						{
							case "lastname":
								user.LastName = property.Value;
								break;
							case "firstname":
								user.FirstName = property.Value;
								break;
							case "middlename":
								user.MiddleName = property.Value;
								break;
							case "telephonenumber":
								user.TelephoneNumber = property.Value;
								break;
							case "islead":
								if (bool.TryParse(property.Value, out bool isLead))
								{
									user.IsLead = isLead;
								}
								else
								{
									Logger?.Warn($"Некорректное значение для свойства IsLead: {property.Value}");
								}
								break;
							case "passwordhash":
							case "hashpassword":
								if (userPassword == null)
								{
									var password = new Password
									{
										UserLogin = userLogin,
										PasswordHash = property.Value
									};

									_context.Passwords.Add(password);
								}
								else
								{
									userPassword.PasswordHash = property.Value;
								}
								break;
							default:
								//Logger?.Warn($"Свойство {property.Name} не распознано и не может быть обновлено!");
								break;
						}
					}

					_context.SaveChanges();
					transaction.Commit();
					//Logger?.Debug($"Свойства пользователя с логином {userLogin} успешно обновлены.");
				}
				catch (Exception ex)
				{
					//Logger?.Error($"Ошибка при обновлении свойств пользователя с логином {userLogin}:\n {ex}");
					transaction.Rollback();
					throw;
				}
			}
		}

		public IEnumerable<Permission> GetAllPermissions()
		{
			var requestRights = _context.RequestRights.AsNoTracking()
				.Select(request => new Permission(request.Id.ToString(), request.Name, "")).ToList();
			var itRoles = _context.ITRoles.AsNoTracking()
				.Select(request => new Permission(request.Id.ToString(), request.Name, "")).ToList();

			return requestRights.Concat(itRoles);
		}

		public void AddUserPermissions(string userLogin, IEnumerable<string> rightIds)
		{
			if (!IsUserExists(userLogin))
			{
				//Logger?.Warn($"Пользователь с логином {userLogin} не найден!");
				throw new ArgumentException($"Пользователь с логином {userLogin} не найден.", nameof(userLogin));
			}

			using (var transaction = _context.Database.BeginTransaction())
			{
				var requestRights = _context.RequestRights.AsNoTracking().Select(r => r.Id).ToList();
				var itRoles = _context.ITRoles.AsNoTracking().Select(r => r.Id).ToList();

				var userCurrentRequestRights = _context.UserRequestRights.AsNoTracking()
					.Where(us => us.UserLogin == userLogin).ToList();
				var userCurrentITRoles = _context.UserITRoles.AsNoTracking()
					.Where(us => us.UserLogin == userLogin).ToList();

				var userRequestRights = new List<UserRequestRight>();
				var userITRoles = new List<UserITRole>();

				foreach (var rightId in rightIds)
				{
					if (Int32.TryParse(rightId.Split(":").Skip(1).First(), out var result))
					{
						if (requestRights.Contains(result)
							&& !userCurrentRequestRights.Any(r => r.RequestRightId == result))
						{
							userRequestRights.Add(new UserRequestRight()
							{
								UserLogin = userLogin,
								RequestRightId = result,
							});
						}

						if (itRoles.Contains(result)
							&& !userCurrentITRoles.Any(r => r.ITRoleId == result))
						{
							userITRoles.Add(new UserITRole()
							{
								UserLogin = userLogin,
								ITRoleId = result,
							});
						}
					}
				}

				try
				{
					_context.UserITRoles.AddRange(userITRoles);
					_context.UserRequestRights.AddRange(userRequestRights);
					_context.SaveChanges();

					transaction.Commit();
					//Logger?.Debug($"Выбранные права были успешно добавлены пользователю с логином {userLogin}!");
				}
				catch (Exception ex)
				{
					transaction.Rollback();
					//Logger?.Error($"Ошибка при обновлении прав пользователя:\n {ex}");
					throw;
				}
			}
		}

		public void RemoveUserPermissions(string userLogin, IEnumerable<string> rightIds)
		{
			if (!IsUserExists(userLogin))
			{
				//Logger?.Warn($"Пользователь с логином {userLogin} не найден!");
				throw new ArgumentException($"Пользователь с логином {userLogin} не найден.", nameof(userLogin));
			}

			using (var transaction = _context.Database.BeginTransaction())
			{
				var requestRights = _context.RequestRights.AsNoTracking().Select(r => r.Id).ToList();
				var itRoles = _context.ITRoles.AsNoTracking().Select(r => r.Id).ToList();

				var userCurrentRequestRights = _context.UserRequestRights.AsNoTracking()
					.Where(us => us.UserLogin == userLogin).ToList();
				var userCurrentITRoles = _context.UserITRoles.AsNoTracking()
					.Where(us => us.UserLogin == userLogin).ToList();

				var userRequestRights = new List<UserRequestRight>();
				var userITRoles = new List<UserITRole>();

				foreach (var rightId in rightIds)
				{
					if (Int32.TryParse(rightId.Split(":").Skip(1).First(), out var result))
					{
						if (requestRights.Contains(result)
						    && userCurrentRequestRights.Any(r => r.RequestRightId == result))
						{
							userRequestRights.Add(new UserRequestRight()
							{
								UserLogin = userLogin,
								RequestRightId = result,
							});
						}

						if (itRoles.Contains(result)
						    && userCurrentITRoles.Any(r => r.ITRoleId == result))
						{
							userITRoles.Add(new UserITRole()
							{
								UserLogin = userLogin,
								ITRoleId = result,
							});
						}
					}
				}

				try
				{
					_context.UserITRoles.RemoveRange(userITRoles);
					_context.UserRequestRights.RemoveRange(userRequestRights);
					_context.SaveChanges();

					transaction.Commit();
					//Logger?.Debug($"Выбранные права были успешно добавлены пользователю с логином {userLogin}!");
				}
				catch (Exception ex)
				{
					transaction.Rollback();
					//Logger?.Error($"Ошибка при обновлении прав пользователя:\n {ex}");
					throw;
				}
			}
		}

		public IEnumerable<string> GetUserPermissions(string userLogin)
		{
			if (!IsUserExists(userLogin))
			{
				Logger?.Warn($"Пользователь с логином {userLogin} не найден!");
				throw new ArgumentException($"Пользователь с логином {userLogin} не найден.", nameof(userLogin));
			}

			return _context.UserRequestRights.AsNoTracking().Where(urr => urr.UserLogin == userLogin)
				.Include(urr => urr.RequestRight).Select(urr => urr.RequestRight.Name).ToList();
		}
	}
}