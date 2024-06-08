namespace Connector.Infrastructure.DataAccess.Models.POCO
{
    public class RolePOCO
    {
        /// <summary>
        /// Уникальный идентификатор
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Название
        /// </summary>
        public required string Name { get; set; }

        /// <summary>
        /// Номер корпоративного телефона
        /// </summary>
        public required string CorporatePhoneNumber { get; set; } = "";
    }
}
