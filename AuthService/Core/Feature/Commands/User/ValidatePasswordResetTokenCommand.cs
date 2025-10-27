using MediatR;

namespace AuthService.Core.Feature.Commands.User
{
    public class ValidatePasswordResetTokenCommand : IRequest<PasswordResetTokenValidationResult>
    {
        public string Token { get; set; }
    }

    public class PasswordResetTokenValidationResult
    {
        public bool IsValid { get; set; }
        public string Email { get; set; }
        public string Message { get; set; }

        public PasswordResetTokenValidationResult(bool isValid, string email = null, string message = null)
        {
            IsValid = isValid;
            Email = email;
            Message = message;
        }
    }
}

