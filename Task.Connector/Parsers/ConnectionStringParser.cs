using System.Text.RegularExpressions;
using Task.Connector.Extensions;
using Task.Connector.Parsers.Records;

namespace Task.Connector.Parsers
{
    public class ConnectionStringParser : IStringParser<ConnectionConfiguration>
    {
        private const string ConnectionString = "ConnectionString";
        private const string Provider = "Provider";

        public ConnectionConfiguration Parse(string input)
        {
            var regexp = new Regex(@"(\w+)\W+([^']+)");
            var matches = regexp.Matches(input);

            var matchesDict = new Dictionary<string, string>();

            foreach (Match match in matches)
                matchesDict.Add(match.Groups[1].Value, match.Groups[2].Value);

            var connectionString = matchesDict.GetValueOrEmpty(ConnectionString);
            var provider = matchesDict.GetValueOrEmpty(Provider);

            return new ConnectionConfiguration(connectionString, provider);
        }
    }
}
