using Connector.Infrastructure.DataAccess.Models.POCO;
using Task.Integration.Data.Models.Models;

namespace Connector.Infrastructure.DataAccess.Maps
{
    public static class RequestPocoToPermission
    {
        private const string groupName = "Request";
        private const string delimeter = ":";

        public static IEnumerable<Permission> Convert(this IEnumerable<RequestPOCO> requests)
        {
            foreach (var request in requests)
            {
                yield return new Permission(
                    GetId(request.Id),
                    request.Name,
                    "");
            }
        }

        public static IEnumerable<string> ConvertRequestIds(this IEnumerable<int> ids)
        {
            foreach (var id in ids)
            {
                yield return GetId(id);
            }
        }

        private static string GetId(int id)
        {
            return string.Join(delimeter, groupName, id);
        }
    }
}
