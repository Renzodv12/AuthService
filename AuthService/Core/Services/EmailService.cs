using AuthService.Core.Interfaces;
using AuthService.Core.Models.Email;
using Microsoft.Extensions.Options;
using System.Net;
using System.Net.Mail;
using System.Text;

namespace AuthService.Core.Services
{
    public class EmailService : IEmailService
    {
        private readonly EmailSettings _emailSettings;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IOptions<EmailSettings> emailSettings, ILogger<EmailService> logger)
        {
            _emailSettings = emailSettings.Value;
            _logger = logger;
        }

        public async Task SendEmailVerificationAsync(string email, string firstName, string verificationToken)
        {
            var subject = "Verificación de Email - AuthService";
            var verificationUrl = $"{_emailSettings.BaseUrl}/verify-email?token={verificationToken}";
            
            var body = $@"
                <html>
                <body>
                    <h2>¡Hola {firstName}!</h2>
                    <p>Gracias por registrarte en nuestro servicio. Para completar tu registro, por favor verifica tu dirección de email haciendo clic en el siguiente enlace:</p>
                    <p><a href='{verificationUrl}' style='background-color: #4CAF50; color: white; padding: 14px 20px; text-decoration: none; border-radius: 4px;'>Verificar Email</a></p>
                    <p>Si no puedes hacer clic en el botón, copia y pega el siguiente enlace en tu navegador:</p>
                    <p>{verificationUrl}</p>
                    <p>Este enlace expirará en 24 horas.</p>
                    <p>Si no creaste esta cuenta, puedes ignorar este email.</p>
                    <br>
                    <p>Saludos,<br>El equipo de AuthService</p>
                </body>
                </html>";

            await SendEmailAsync(email, subject, body);
        }

        public async Task SendPasswordResetAsync(string email, string firstName, string resetToken)
        {
            var subject = "Recuperación de Contraseña - AuthService";
            var resetUrl = $"{_emailSettings.BaseUrl}/reset-password?token={resetToken}";
            
            var body = $@"
                <html>
                <body>
                    <h2>¡Hola {firstName}!</h2>
                    <p>Recibimos una solicitud para restablecer la contraseña de tu cuenta.</p>
                    <p>Para crear una nueva contraseña, haz clic en el siguiente enlace:</p>
                    <p><a href='{resetUrl}' style='background-color: #f44336; color: white; padding: 14px 20px; text-decoration: none; border-radius: 4px;'>Restablecer Contraseña</a></p>
                    <p>Si no puedes hacer clic en el botón, copia y pega el siguiente enlace en tu navegador:</p>
                    <p>{resetUrl}</p>
                    <p>Este enlace expirará en 1 hora.</p>
                    <p>Si no solicitaste este cambio, puedes ignorar este email y tu contraseña permanecerá sin cambios.</p>
                    <br>
                    <p>Saludos,<br>El equipo de AuthService</p>
                </body>
                </html>";

            await SendEmailAsync(email, subject, body);
        }

        public async Task SendWelcomeEmailAsync(string email, string firstName)
        {
            var subject = "¡Bienvenido a AuthService!";
            
            var body = $@"
                <html>
                <body>
                    <h2>¡Bienvenido {firstName}!</h2>
                    <p>Tu cuenta ha sido verificada exitosamente.</p>
                    <p>Ya puedes comenzar a usar todos nuestros servicios.</p>
                    <p>Si tienes alguna pregunta, no dudes en contactarnos.</p>
                    <br>
                    <p>Saludos,<br>El equipo de AuthService</p>
                </body>
                </html>";

            await SendEmailAsync(email, subject, body);
        }

        private async Task SendEmailAsync(string toEmail, string subject, string body)
        {
            try
            {
                using var client = new SmtpClient(_emailSettings.SmtpServer, _emailSettings.SmtpPort)
                {
                    Credentials = new NetworkCredential(_emailSettings.SmtpUsername, _emailSettings.SmtpPassword),
                    EnableSsl = _emailSettings.EnableSsl
                };

                var mailMessage = new MailMessage
                {
                    From = new MailAddress(_emailSettings.FromEmail, _emailSettings.FromName),
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = true
                };

                mailMessage.To.Add(toEmail);

                await client.SendMailAsync(mailMessage);
                _logger.LogInformation("Email enviado exitosamente a {Email}", toEmail);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al enviar email a {Email}", toEmail);
                throw;
            }
        }
    }
}