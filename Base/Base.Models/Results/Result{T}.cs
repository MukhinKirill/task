namespace Base.Models.Results
{
    /// <summary>
    /// Типизированный результат выполнения
    /// </summary>
    /// <typeparam name="T">Тип данных</typeparam>
    public class Result<T> : Result
    {
        public Result()
        {

        }

        public Result(T value)
        {
            Value = value;
            IsError = false;
        }

        public Result(string message) : base(message)
        {
        }

        /// <summary>
        /// Значение
        /// </summary>
        public T Value
        {
            get { return (T) ResultValue;}
            set { ResultValue = value; }
        }
        
        /// <summary>
        /// Клонирование Generic типа
        /// Само значение не клонируется
        /// </summary>
        /// <typeparam name="TY">Новый тип</typeparam>
        /// <returns></returns>
        public Result<TY> Clone<TY>()
        {
            return new Result<TY>
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
        public static Result<T> CreateSuccessResult(T value = default, string message = null)
        {
            return new Result<T> {Value = value, Message = message, IsError = false};
        }

        /// <summary>
        /// Создание неуспешного результата
        /// </summary>
        /// <param name="value"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public static Result<T> CreateErrorResult(T value = default, string message = null)
        {
            return new Result<T> { Value = value, Message = message, IsError = true};
        }
    }
}