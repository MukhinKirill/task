namespace Core.Models.Options
{
    public class DbOptions
    {
        /// <summary>
        /// Строка подключения
        /// </summary>
        public required string ConnectionString { get; set; }

        /// <summary>
        /// Название схемы
        /// </summary>
        public required string Schema { get; set; }

        /// <summary>
        /// Время на команду
        /// </summary>
        public int CommandTimeOut { get; set; }
    }
}
