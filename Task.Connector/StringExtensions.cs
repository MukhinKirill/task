using System.Text.RegularExpressions;

namespace Task.Connector;

internal static class StringExtensions
{
	internal static Dictionary<string, string> ParseConnectionString(this string connectionString)
	{
		var result = new Dictionary<string, string>();

		// (\w+) - >= 1 буквенно-цифровых символов (ключ)
		// '([^']+)' - >= 1 символов в одинарных кавычках кроме самих кавычек (значение)
		var pattern = @"(\w+)='([^']+)'";

		var matches = Regex.Matches(connectionString, pattern);

		foreach (Match match in matches)
		{
			var key = match.Groups[1].Value;
			var value = match.Groups[2].Value;

			result[key] = value;
		}

		return result;
	}
}
