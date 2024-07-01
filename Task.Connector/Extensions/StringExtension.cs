namespace Task.Connector.Extensions
{
    public static class StringExtension
    {
        public static bool EqualsIgnoreCase(this string @string, string otherString)
        {
            return @string.ToLower() == otherString.ToLower();
        }
    }
}
