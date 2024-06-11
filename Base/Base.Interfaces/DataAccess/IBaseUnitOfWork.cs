namespace Base.Interfaces.DataAccess
{
    /// <summary>
    /// Базовый интерфейс для работы с единицей работы
    /// </summary>
    public interface IBaseUnitOfWork : IDisposable
    {
        /// <summary>
        /// Начать транзакцию
        /// </summary>
        void BeginTran();

        /// <summary>
        /// Коммит
        /// </summary>
        void Commit();

        /// <summary>
        /// Откат
        /// </summary>
        void RollBack();
    }
}
