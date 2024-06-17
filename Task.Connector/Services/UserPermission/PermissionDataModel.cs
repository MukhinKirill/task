namespace Task.Connector.Services.UserPermission
{
    public class PermissionDataModel
    {
        public int Id { get; set; }
        public string Type { get; set; }

        public PermissionDataModel(int id, string type)
        {
            if (id <= 0 || string.IsNullOrWhiteSpace(type))
            {
                throw new ArgumentException("Permission data invalid arguments");
            }

            (Id, Type) = (id, type);
        }
    }
}