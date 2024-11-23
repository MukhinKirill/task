using FluentValidation;
using Task.Connector.Models;

namespace Task.Connector.Validation
{
    internal class PasswordValidator : AbstractValidator<Password>
    {
        public PasswordValidator()
        {
            RuleFor(password => password.UserId)
                .NotEmpty().WithMessage("Логин пользователя не может быть пустым.")
                .NotNull().WithMessage("Пользователь должен быть указан.")
                .MaximumLength(22).WithMessage("Логин не может превышать 22 символа.");

            RuleFor(password => password.hashPassword)
                .NotEmpty().WithMessage("Пароль не может быть пустым.")
                .NotNull().WithMessage("Пароль обязателен.")
                .MaximumLength(20).WithMessage("Пароль не может превышать 20 символов.");
        }
    }
}
