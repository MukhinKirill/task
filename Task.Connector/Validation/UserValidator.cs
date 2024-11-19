using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Task.Integration.Data.DbCommon.DbModels;

namespace Task.Connector.Validation
{
    internal class UserValidator : AbstractValidator<User>
    {
        public UserValidator()
        {

            RuleFor(user => user.Login)
            .NotEmpty().WithMessage("Логин не может быть пустым.")
            .NotNull().WithMessage("Логин обязателен.")
            .MaximumLength(22).WithMessage("Логин не может превышать 22 символа.");

            RuleFor(user => user.LastName)
                .MaximumLength(20).WithMessage("Фамилия не может превышать 20 символов.");

            RuleFor(user => user.FirstName)
                .MaximumLength(20).WithMessage("Имя не может превышать 20 символов.");

            RuleFor(user => user.MiddleName)
                .MaximumLength(20).WithMessage("Отчество не может превышать 20 символов.");

            RuleFor(user => user.TelephoneNumber)
                .MaximumLength(20).WithMessage("Номер телефона не может превышать 20 символов.");

            RuleFor(user => user.IsLead)
                .NotNull().WithMessage("парметр IsLead обязателен.");
        }
    }
}
