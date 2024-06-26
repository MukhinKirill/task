namespace Task.Connector.Extensions
{
    public static class DictionaryExtension
    {
        public static string GetValueOrEmpty(this Dictionary<string, string> dict, string key)
        {
            if (dict.ContainsKey(key))
            {
                return dict[key];
            }

            return string.Empty;
        }
    }
}
