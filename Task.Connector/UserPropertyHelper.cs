using System.ComponentModel.DataAnnotations;
using Task.Integration.Data.DbCommon.DbModels;
using Task.Integration.Data.Models.Models;

namespace Task.Connector
{
	public static class UserPropertyHelper
	{
		private static List<UserProperty>? _userProperties;

		public static IReadOnlyCollection<UserProperty> GetProperties(User? user = null)
		{
			if (_userProperties is not null && user is null)
			{
				return _userProperties;
			}

			var userType = typeof(User);
			var properties = userType
				.GetProperties()
				.Where(pi => pi.CustomAttributes.All(a => a.AttributeType != typeof(KeyAttribute)))
				.Select(pi => new UserProperty(pi.Name, user is null ? string.Empty : pi.GetValue(user, null)?.ToString() ?? string.Empty))
				.ToList();

			if (user is null)
			{
				properties.Add(new UserProperty(nameof(Sequrity.Password), string.Empty));
				_userProperties = properties;
			}

			return properties;
		}

		public static void UpdateProperties(User user, IEnumerable<UserProperty> userProperties)
		{
			var properties = userProperties.ToDictionary(x => x.Name.ToLower(), x => x.Value);
			user.IsLead = properties.GetValueOrDefault(nameof(user.IsLead).ToLower()) == "true";
			user.TelephoneNumber = properties.GetValueOrDefault(nameof(user.TelephoneNumber).ToLower()) ?? user.TelephoneNumber ?? "";
			user.FirstName = properties.GetValueOrDefault(nameof(user.FirstName).ToLower()) ?? user.FirstName ?? "";
			user.LastName = properties.GetValueOrDefault(nameof(user.LastName).ToLower()) ?? user.LastName ?? "";
			user.MiddleName = properties.GetValueOrDefault(nameof(user.MiddleName).ToLower()) ?? user.MiddleName ?? "";
		}
	}
}
