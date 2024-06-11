namespace Connector.Infrastructure.DataAccess.Utils
{
    public static class GetFilterRow
    {
        public static (string name, object value, string rowFilter) FilterToDataAccess(this IEnumerable<int> ids)
        {
            if (ids.Count() == 1)
            {
                return ("id", ids.First(), "@id");
            }

            return ("ids", ids.ToArray(), "ANY(@ids)");
        }
    }
}
