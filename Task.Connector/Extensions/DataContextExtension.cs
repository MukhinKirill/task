using System.ComponentModel.DataAnnotations;
using System.Reflection;
using Task.Integration.Data.DbCommon.DbModels;
using System.Collections.Generic;


namespace Task.Connector.Extensions
{
    public static class DataContextExtension
    {
        private const char delimeter = ':';
        private const string requestRightModel = "Request";
        private const string itRoleRightModel = "Role";

        public static Dictionary<string, PropertyInfo> GetDbModelProperties(Type type)
        {
            Dictionary<string, PropertyInfo> userProperties = type.GetProperties().Where(x => x.GetCustomAttributes(typeof(KeyAttribute), false).Length == 0).ToDictionary(x => x.Name.ToLower(), x => x);

            return userProperties;
        }

        public static (IEnumerable<UserRequestRight>, IEnumerable<UserITRole>) GetPermissionRange(IEnumerable<string> rightIds, string userLogin)
        {

            HashSet<UserRequestRight> userRequestRights = new();
            HashSet<UserITRole> userITRoles = new();

            foreach (string rightId in rightIds)
            {
                string[] rightsData = rightId.Split(delimeter);

                if (rightsData.Length == 2 && Int32.TryParse(rightsData[1], out int rightIdInt))
                {
                    switch (rightsData[0])
                    {
                        case requestRightModel:
                            userRequestRights.Add(new UserRequestRight()
                            {
                                UserId = userLogin,
                                RightId = rightIdInt
                            });
                            break;
                        case itRoleRightModel:
                            userITRoles.Add(new UserITRole()
                            {
                                UserId = userLogin,
                                RoleId = rightIdInt
                            });
                            break;
                    }
                }
            }

            return (userRequestRights, userITRoles);
        }
    }
}
