namespace Task.Connector.Models
{
    // Сущность описывает конкретные права, присвоенные некому пользователю
    public class UserPermission
    {
        public required string PermissionTypeId { get; set; }

        public required string UserId { get; set; }
    }
}
