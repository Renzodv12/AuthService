using AuthService.Core.Feature.Commands.User;
using AuthService.Core.Interfaces;
using MediatR;
using AuthService.Core.Entities;
using AuthService.Core.Utils;
using AuthService.Core.Exceptions;
using Hangfire;
using System.Security.Cryptography;
namespace AuthService.Core.Feature.Handler.User
{
    public class RegisterCommandHandler : IRequestHandler<RegisterCommand>
    {
        private readonly IRepository<AuthService.Core.Entities.User> _userRepository;
        private readonly IRepository<EmailVerificationToken> _emailVerificationRepository;
        private readonly IEmailBackgroundService _emailBackgroundService;
        
        public RegisterCommandHandler(
            IRepository<AuthService.Core.Entities.User> userRepository,
            IRepository<EmailVerificationToken> emailVerificationRepository,
            IEmailBackgroundService emailBackgroundService)
        {
            _userRepository = userRepository;
            _emailVerificationRepository = emailVerificationRepository;
            _emailBackgroundService = emailBackgroundService;
        }
        public async Task Handle(RegisterCommand request, CancellationToken cancellationToken)
        {
            var verifyUsuario = await _userRepository.FirstOrDefaultAsync(x => x.Email == request.register.Email || x.CI == request.register.CI);
            if(verifyUsuario == null) {
                var salt = PasswordHelper.GenerateSalt();
                var hashedPassword = PasswordHelper.HashPassword(request.register.Password, salt);
                var user = new AuthService.Core.Entities.User
                {
                    Id = Guid.NewGuid(),
                    FirstName = request.register.FirstName,
                    LastName = request.register.LastName,
                    Email = request.register.Email,
                    Password = hashedPassword,
                    Salt = salt,
                    CI = request.register.CI,
                    BirthDate =  DateTime.SpecifyKind(request.register.BirthDate, DateTimeKind.Utc),
                    TypeAuth = Enums.TypeAuth.Password,
                    EmailVerified = false,
                    CreateDate = DateTime.UtcNow,
                    LastModifiedDate = DateTime.UtcNow
                };

                await _userRepository.AddAsync(user);
                await _userRepository.SaveChangesAsync();

                // Crear token de verificación de email
                var verificationToken = GenerateSecureToken();
                var emailVerification = new EmailVerificationToken
                {
                    Id = Guid.NewGuid(),
                    UserId = user.Id,
                    Token = verificationToken,
                    Email = user.Email,
                    ExpiresAt = DateTime.UtcNow.AddHours(24),
                    IsUsed = false,
                    CreatedAt = DateTime.UtcNow
                };

                await _emailVerificationRepository.AddAsync(emailVerification);
                await _emailVerificationRepository.SaveChangesAsync();

                // Enviar email de verificación usando Hangfire
                BackgroundJob.Enqueue(() => _emailBackgroundService.SendEmailVerificationAsync(
                    user.Email, user.FirstName, verificationToken));
            }
            else
            {
                throw new UserAlreadyExistsException(request.register.Email, request.register.CI);
            }
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
