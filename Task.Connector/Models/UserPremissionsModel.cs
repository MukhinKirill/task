using Task.Connector.Constants;

namespace Task.Connector.Models
{
    internal class UserPremissionsModel
    {
        public IReadOnlyCollection<int> Requests { get; }

        public IReadOnlyCollection<int> Roles { get;  }

        public UserPremissionsModel(IEnumerable<string> rightIds)
        {
            var requests = new List<int>();
            var roles = new List<int>();

            foreach (var id in rightIds)
            {
                var parameter = id.Split(RightConstants.DELIMETER);
                var parameterId = int.Parse(parameter[1]);
                if (parameter[0] == RightConstants.IT_ROLE_RIGHT_GROUP_NAME)
                    roles.Add(parameterId);
                else if (parameter[0] == RightConstants.REQUEST_RIGHT_GROUP_NAME)
                    requests.Add(parameterId);
            }

            Requests = requests;
            Roles = roles;
        }
    }
}
