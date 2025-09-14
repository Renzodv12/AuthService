using AuthService.Core.Feature.Commands.User;
using AuthService.Core.Interfaces;
using MediatR;
using AuthService.Core.Entities;
using Hangfire;

namespace AuthService.Core.Feature.Handler.User
{
    public class VerifyEmailCommandHandler : IRequestHandler<VerifyEmailCommand, bool>
    {
        private readonly IRepository<EmailVerificationToken> _emailVerificationRepository;
        private readonly IRepository<AuthService.Core.Entities.User> _userRepository;
        private readonly IEmailBackgroundService _emailBackgroundService;

        public VerifyEmailCommandHandler(
            IRepository<EmailVerificationToken> emailVerificationRepository,
            IRepository<AuthService.Core.Entities.User> userRepository,
            IEmailBackgroundService emailBackgroundService)
        {
            _emailVerificationRepository = emailVerificationRepository;
            _userRepository = userRepository;
            _emailBackgroundService = emailBackgroundService;
        }

        public async Task<bool> Handle(VerifyEmailCommand request, CancellationToken cancellationToken)
        {
            var verificationToken = await _emailVerificationRepository.FirstOrDefaultAsync(
                x => x.Token == request.Token && x.Email == request.Email && !x.IsUsed);

            if (verificationToken == null || verificationToken.IsExpired)
            {
                return false;
            }

            var user = await _userRepository.FirstOrDefaultAsync(x => x.Id == verificationToken.UserId);
            if (user == null)
            {
                return false;
            }

            // Marcar el token como usado
            verificationToken.IsUsed = true;
            verificationToken.UsedAt = DateTime.UtcNow;
            await _emailVerificationRepository.SaveChangesAsync();

            // Marcar el email como verificado
            user.EmailVerified = true;
            user.LastModifiedDate = DateTime.UtcNow;
            await _userRepository.SaveChangesAsync();

            // Enviar email de bienvenida
            BackgroundJob.Enqueue(() => _emailBackgroundService.SendWelcomeEmailAsync(
                user.Email, user.FirstName));

            return true;
        }
    }
}