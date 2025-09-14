using AuthService.Core.Interfaces;
using Hangfire;

namespace AuthService.Core.Services
{
    public class EmailBackgroundService : IEmailBackgroundService
    {
        private readonly IEmailService _emailService;
        private readonly ILogger<EmailBackgroundService> _logger;

        public EmailBackgroundService(IEmailService emailService, ILogger<EmailBackgroundService> logger)
        {
            _emailService = emailService;
            _logger = logger;
        }

        public async Task SendEmailVerificationAsync(string email, string firstName, string verificationToken)
        {
            try
            {
                _logger.LogInformation("Enviando email de verificación a {Email}", email);
                await _emailService.SendEmailVerificationAsync(email, firstName, verificationToken);
                _logger.LogInformation("Email de verificación enviado exitosamente a {Email}", email);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al enviar email de verificación a {Email}", email);
                throw;
            }
        }

        public async Task SendPasswordResetAsync(string email, string firstName, string resetToken)
        {
            try
            {
                _logger.LogInformation("Enviando email de recuperación de contraseña a {Email}", email);
                await _emailService.SendPasswordResetAsync(email, firstName, resetToken);
                _logger.LogInformation("Email de recuperación de contraseña enviado exitosamente a {Email}", email);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al enviar email de recuperación de contraseña a {Email}", email);
                throw;
            }
        }

        public async Task SendWelcomeEmailAsync(string email, string firstName)
        {
            try
            {
                _logger.LogInformation("Enviando email de bienvenida a {Email}", email);
                await _emailService.SendWelcomeEmailAsync(email, firstName);
                _logger.LogInformation("Email de bienvenida enviado exitosamente a {Email}", email);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al enviar email de bienvenida a {Email}", email);
                throw;
            }
        }
    }
}