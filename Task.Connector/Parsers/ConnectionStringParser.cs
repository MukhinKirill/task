using System.Text;
using System.Text.RegularExpressions;
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

            var connectionString = string.Empty;
            var provider = string.Empty;

            foreach (Match match in matches)
            {
                if (match.Groups[1].Value.ToLower() == ConnectionString.ToLower())
                {
                    connectionString = match.Groups[2].Value;
                }
                else if(match.Groups[1].Value.ToLower() == Provider.ToLower())
                {
                    provider = match.Groups[2].Value;
                }

                if(connectionString != string.Empty && provider != string.Empty)
                {
                    break;
                }
            }
            
            return new ConnectionConfiguration(connectionString, provider);
        }
    }
}
