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
            .NotEmpty().WithMessage("Login не может быть пустым.")
            .NotNull().WithMessage("Login обязателен.")
            .MaximumLength(22).WithMessage("Login не может превышать 22 символа.");

            RuleFor(user => user.LastName)
                .MaximumLength(20).WithMessage("LastName не может превышать 20 символов.");

            RuleFor(user => user.FirstName)
                .MaximumLength(20).WithMessage("FirstName не может превышать 20 символов.");

            RuleFor(user => user.MiddleName)
                .MaximumLength(20).WithMessage("MiddleName не может превышать 20 символов.");

            RuleFor(user => user.TelephoneNumber)
                .MaximumLength(20).WithMessage("TelephoneNumber не может превышать 20 символов.");

            RuleFor(user => user.IsLead)
                .NotNull().WithMessage("IsLead обязателен.");
        }
    }
}
