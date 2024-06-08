namespace Base.Models.Results
{
    /// <summary>
    /// Результат выполнения
    /// </summary>
    public class Result
    {
        public Result(object resultValue)
        {
            ResultValue = resultValue;
        }

        public Result()
        {

        }

        public Result(string message)
        {
            Message = message;
            IsError = true;
        }

        /// <summary>
        /// Ошибка
        /// </summary>
        public virtual bool IsError { get; set; }

        /// <summary>
        /// Сообщение
        /// </summary>
        public virtual string Message { get; set; }

        /// <summary>
        /// Значение
        /// </summary>
        public virtual object ResultValue { get; set; }

        /// <summary>
        /// Создание успешного результата
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public static Result CreateSuccessResult(string message = null)
        {
            return new Result { Message = message, IsError = false };
        }

        /// <summary>
        /// Создание неуспешного результата
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public static Result CreateErrorResult(string message = null)
        {
            return new Result { Message = message, IsError = true };
        }
    }
}