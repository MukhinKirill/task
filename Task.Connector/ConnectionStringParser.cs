using System.Text;
using System.Text.RegularExpressions;

namespace Task.Connector
{
    public class ConnectionStringParser
    {
        private readonly Dictionary<string, string> _keyValues = new();

        public ConnectionStringParser()
        {
            InitializeDictionary();
        }

        public string Parse(string input)
        {
            input = Regex.Match(input, "ConnectionString='([^']*)", RegexOptions.IgnoreCase).Groups[1].Value;
            var regexp = new Regex(@"(\w+)\s*=([^;]*)");

            foreach (Match match in regexp.Matches(input))
            {
                var key = match.Groups[1].Value;
                var value = match.Groups[2].Value;

                if (_keyValues.ContainsKey(key))
                {
                    _keyValues[key] = value;
                }
            }

            return ConvertDictToString(_keyValues);
        }

        private string ConvertDictToString(Dictionary<string, string> keyValues)
        {
            var stringBuilder = new StringBuilder();

            foreach (var key in keyValues.Keys)
            {
                stringBuilder.Append(key);
                stringBuilder.Append('=');
                stringBuilder.Append(keyValues[key]);
                stringBuilder.Append(";");
            }

            return stringBuilder.ToString();
        }

        private void InitializeDictionary()
        {
            _keyValues.Add("Server", string.Empty);
            _keyValues.Add("Port", string.Empty);
            _keyValues.Add("Database", string.Empty);
            _keyValues.Add("Username", string.Empty);
            _keyValues.Add("Password", string.Empty);
        }
    }
}
