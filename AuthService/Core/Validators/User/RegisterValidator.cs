using AuthService.Core.Models.User;
using FluentValidation;

namespace AuthService.Core.Validators.User
{
    public class RegisterValidator : AbstractValidator<Register>
    {
        public RegisterValidator()
        {
            RuleFor(x => x.FirstName)
                .NotEmpty();

            RuleFor(x => x.LastName)
                .NotEmpty();

            RuleFor(x => x.Email)
                .NotEmpty()
                .EmailAddress();

            RuleFor(x => x.Password)
                .NotEmpty()
                .MinimumLength(6)
                .Matches("[A-Z]").WithMessage("Password must contain at least one uppercase letter.")
                .Matches("[a-z]").WithMessage("Password must contain at least one lowercase letter.")
                .Matches("[0-9]").WithMessage("Password must contain at least one number.")
                .Matches("[^a-zA-Z0-9]").WithMessage("Password must contain at least one special character.");

            RuleFor(x => x.ConfirmPassword)
                .NotEmpty()
                .Equal(x => x.Password).WithMessage("Passwords do not match.");

            RuleFor(x => x.CI)
                .NotEmpty()
                .MinimumLength(4)
                .MaximumLength(20);

            RuleFor(x => x.BirthDate)
                .NotEmpty()
                .LessThan(DateTime.UtcNow).WithMessage("BirthDate must be in the past.");
        }
    }
}
