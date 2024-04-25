namespace Task.Connector
{
    /// <summary>
    /// Содержит информацию об ID для Permission
    /// </summary>
    public class PermissionId
    {
        private const string delimiter = ":";
        private const string requestRightGroupName = "Request";
        private const string itRoleRightGroupName = "Role";
        private readonly int _id;
        private readonly string _groupName;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="rightId">Строка в формате "[название группы (Role или Request)]:[значение ID]", например "Role:1" или "Request:1"</param>
        public PermissionId(string rightId)
        {
            var parts = rightId.Split(delimiter);
            _id = int.Parse(parts[1]);
            _groupName = parts[0];
        }

        /// <summary>
        /// Значение id
        /// </summary>
        public int Id => _id;

        /// <summary>
        /// Принадлежит ли к группе ITRole
        /// </summary>
        public bool IsRoleId => _groupName == itRoleRightGroupName;
        
        /// <summary>
        /// Принадлежит ли к группе RequestRight
        /// </summary>
        public bool IsRequestRightId => _groupName == requestRightGroupName;
    }
}
