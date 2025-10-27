using AuthService.Core.Entities;
using AuthService.Core.Feature.Commands.User;
using AuthService.Core.Interfaces;
using MediatR;

namespace AuthService.Core.Feature.Handler.User
{
    public class ValidatePasswordResetTokenCommandHandler : IRequestHandler<ValidatePasswordResetTokenCommand, PasswordResetTokenValidationResult>
    {
        private readonly IRepository<PasswordResetToken> _passwordResetRepository;

        public ValidatePasswordResetTokenCommandHandler(IRepository<PasswordResetToken> passwordResetRepository)
        {
            _passwordResetRepository = passwordResetRepository;
        }

        public async Task<PasswordResetTokenValidationResult> Handle(ValidatePasswordResetTokenCommand request, CancellationToken cancellationToken)
        {
            var resetToken = await _passwordResetRepository.FirstOrDefaultAsync(
                x => x.Token == request.Token && !x.IsUsed);

            if (resetToken == null)
            {
                return new PasswordResetTokenValidationResult(
                    false, 
                    null, 
                    "Token no válido o no encontrado");
            }

            if (resetToken.IsExpired)
            {
                return new PasswordResetTokenValidationResult(
                    false, 
                    null, 
                    "El token ha expirado. Por favor, solicita un nuevo enlace.");
            }

            return new PasswordResetTokenValidationResult(
                true, 
                resetToken.Email, 
                "Token válido");
        }
    }
}

