using AuthService.Core.Feature.Commands.User;
using AuthService.Core.Interfaces;
using MediatR;
using AuthService.Core.Entities;
using AuthService.Core.Utils;

namespace AuthService.Core.Feature.Handler.User
{
    public class ResetPasswordCommandHandler : IRequestHandler<ResetPasswordCommand, bool>
    {
        private readonly IRepository<PasswordResetToken> _passwordResetRepository;
        private readonly IRepository<AuthService.Core.Entities.User> _userRepository;

        public ResetPasswordCommandHandler(
            IRepository<PasswordResetToken> passwordResetRepository,
            IRepository<AuthService.Core.Entities.User> userRepository)
        {
            _passwordResetRepository = passwordResetRepository;
            _userRepository = userRepository;
        }

        public async Task<bool> Handle(ResetPasswordCommand request, CancellationToken cancellationToken)
        {
            var resetToken = await _passwordResetRepository.FirstOrDefaultAsync(
                x => x.Token == request.Token && x.Email == request.Email && !x.IsUsed);

            if (resetToken == null || resetToken.IsExpired)
            {
                return false;
            }

            var user = await _userRepository.FirstOrDefaultAsync(x => x.Id == resetToken.UserId);
            if (user == null)
            {
                return false;
            }

            // Marcar el token como usado
            resetToken.IsUsed = true;
            resetToken.UsedAt = DateTime.UtcNow;
            await _passwordResetRepository.SaveChangesAsync();

            // Actualizar la contraseña del usuario
            var newSalt = PasswordHelper.GenerateSalt();
            var hashedPassword = PasswordHelper.HashPassword(request.NewPassword, newSalt);
            
            user.Password = hashedPassword;
            user.Salt = newSalt;
            user.LastModifiedDate = DateTime.UtcNow;
            await _userRepository.SaveChangesAsync();

            return true;
        }
    }
}