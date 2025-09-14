using AuthService.Core.Feature.Commands.User;
using AuthService.Core.Interfaces;
using MediatR;
using AuthService.Core.Entities;
using Hangfire;
using System.Security.Cryptography;

namespace AuthService.Core.Feature.Handler.User
{
    public class RequestPasswordResetCommandHandler : IRequestHandler<RequestPasswordResetCommand, bool>
    {
        private readonly IRepository<AuthService.Core.Entities.User> _userRepository;
        private readonly IRepository<PasswordResetToken> _passwordResetRepository;
        private readonly IEmailBackgroundService _emailBackgroundService;

        public RequestPasswordResetCommandHandler(
            IRepository<AuthService.Core.Entities.User> userRepository,
            IRepository<PasswordResetToken> passwordResetRepository,
            IEmailBackgroundService emailBackgroundService)
        {
            _userRepository = userRepository;
            _passwordResetRepository = passwordResetRepository;
            _emailBackgroundService = emailBackgroundService;
        }

        public async Task<bool> Handle(RequestPasswordResetCommand request, CancellationToken cancellationToken)
        {
            var user = await _userRepository.FirstOrDefaultAsync(x => x.Email == request.Email);
            
            // Siempre retornamos true por seguridad, incluso si el usuario no existe
            if (user == null)
            {
                return true;
            }

            // Invalidar tokens anteriores
            var existingTokens = (await _passwordResetRepository.GetAllAsync())
                .Where(x => x.UserId == user.Id && !x.IsUsed && x.ExpiresAt > DateTime.UtcNow);
            
            foreach (var token in existingTokens)
            {
                token.IsUsed = true;
                token.UsedAt = DateTime.UtcNow;
            }
            await _passwordResetRepository.SaveChangesAsync();

            // Crear nuevo token de recuperación
            var resetToken = GenerateSecureToken();
            var passwordReset = new PasswordResetToken
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                Token = resetToken,
                Email = user.Email,
                ExpiresAt = DateTime.UtcNow.AddHours(1), // 1 hora de validez
                IsUsed = false,
                CreatedAt = DateTime.UtcNow
            };

            await _passwordResetRepository.AddAsync(passwordReset);
            await _passwordResetRepository.SaveChangesAsync();

            // Enviar email de recuperación usando Hangfire
            BackgroundJob.Enqueue(() => _emailBackgroundService.SendPasswordResetAsync(
                user.Email, user.FirstName, resetToken));

            return true;
        }

        private string GenerateSecureToken()
        {
            using var rng = RandomNumberGenerator.Create();
            var bytes = new byte[32];
            rng.GetBytes(bytes);
            return Convert.ToBase64String(bytes).Replace("+", "-").Replace("/", "_").Replace("=", "");
        }
    }
}