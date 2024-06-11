namespace Connector.Infrastructure.DataAccess.Models.POCO
{
    public class RequestPOCO
    {
        /// <summary>
        /// Уникальный идентификатор
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Название
        /// </summary>
        public required string Name { get; set; }
    }
}
