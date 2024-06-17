using System.Reflection;
using System.Text.RegularExpressions;

namespace Task.Connector.Services.UserPermission
{
    public static class PermissionHelper
    {
        public static string Delimiter { get; } = ":";
        private static string dataPattern = $@"^(\w+){Delimiter}\d+$";
        private static readonly string[] permissionTypes;

        static PermissionHelper()
        {
            permissionTypes = typeof(PermissionTypes).GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)
                .Where(field => field.IsLiteral && !field.IsInitOnly)
                .Select(field => field.GetValue(null).ToString())
                .ToArray();
        }

        public static bool IsPermissionDataValid(string permissionData)
        {
            var match = Regex.Match(permissionData, dataPattern);
            if (match.Success)
            {
                var permissionType = match.Groups[1].Value;
                return permissionTypes.Contains(permissionType);
            }

            return false;
        }

        public static PermissionDataModel SplitPermissionData(string permissionData)
        {
            var permissionDataSplitted = permissionData.Split(Delimiter);
            var permissionDataModel = new PermissionDataModel(Convert.ToInt32(permissionDataSplitted[1]), permissionDataSplitted[0]);
            return permissionDataModel;
        }
    }
}