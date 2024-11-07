namespace Task.Connector
{
    public class UserPermission
    {
        public static (List<int> requestRightIds, List<int> itRoleIds) ParseRightId(IEnumerable<string> rightIds)
        {
            var requestRightIds = new List<int>();
            var itRoleIds = new List<int>();

            foreach (var rightId in rightIds)
            {
                var parts = rightId.Split(':');
                if (parts.Length == 2 && int.TryParse(parts[1], out int id))
                {
                    if (parts[0] == "Role")
                    {
                        itRoleIds.Add(id);
                    }
                    else if (parts[0] == "Request")
                    {
                        requestRightIds.Add(id);
                    }
                }
            }

            return (requestRightIds, itRoleIds);
        }
    }
}
