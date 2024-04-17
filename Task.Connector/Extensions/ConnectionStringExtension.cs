using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Task.Connector.Extensions
{
    public static class ConnectionStringExtension
    {
        public static string GetDbConnectionString (this string connectionString)
        {
            Dictionary<string, string> attributes = connectionString.GetAttributes();
            return attributes["ConnectionString"];
        }
        public static string GetProviderName(this string connectionString)
        {
            Dictionary<string, string> attributes = connectionString.GetAttributes();
            if (attributes["Provider"].Contains("SqlServer"))
                return "MSSQL";
            else if (attributes["Provider"].Contains("PostgreSQL"))
                return "POSTGRE";
            else return "UnknownProvider";
        }
        public static Dictionary<string, string> GetAttributes(this string _string)
        {
            Dictionary<string, string> attributes = new Dictionary<string, string>();
            string[] valuePairs = _string.Split(';');
            foreach (string pair in valuePairs)
            {
                string key = pair.Substring(0, pair.IndexOf('=')).Trim();
                string val = pair.Substring(pair.IndexOf('=')+1).Trim('\'');
                attributes[key] = val;
            }
            return attributes;
        }
    }
}
