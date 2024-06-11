namespace Base.Models.Results
{
    /// <summary>
    /// Списковый результат выполнения
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ListResult<T> : Result<IEnumerable<T>>
    {
        public ListResult()
        {
            
        }

        public ListResult(IEnumerable<T> items) : base(items)
        {

        }
        public ListResult(string message) : base(message)
        {
        }

        public IEnumerable<T> Items
        {
            get { return Value; }
            set { Value = new List<T>(value); }
        }

        /// <summary>
        /// Клонирование Generic типа
        /// Само значение не клонируется
        /// </summary>
        /// <typeparam name="TY">Новый тип</typeparam>
        /// <returns></returns>
        public new ListResult<TY> Clone<TY>()
        {
            return new ListResult<TY>
            {
                IsError = IsError,
                Message = Message
            };
        }

        /// <summary>
        /// Создание успешного результата
        /// </summary>
        /// <param name="value"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public static ListResult<T> CreateSuccessListResult(IEnumerable<T> value = default, string message = null)
        {
            return new ListResult<T> { Value = new List<T>(value), Message = message, IsError = false };
        }

        /// <summary>
        /// Создание неуспешного результата
        /// </summary>
        /// <param name="value"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public static ListResult<T> CreateErrorListResult(IEnumerable<T> value = default, string message = null)
        {
            return new ListResult<T>{ Value = value==null?null: new List<T>(value), Message = message, IsError = true };
        }
    }
}