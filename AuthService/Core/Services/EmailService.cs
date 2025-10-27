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
            var frontendUrl = _emailSettings.FrontendBaseUrl ?? _emailSettings.BaseUrl;
            var resetUrl = $"{frontendUrl}/auth/custom/reset-password?token={resetToken}&email={Uri.EscapeDataString(email)}";
            
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

        public async Task SendTwoFactorCodeAsync(string email, string firstName, string code)
        {
            var subject = "Código de verificación - AuthService";
            
            var body = $@"
                <html>
                <body style='font-family: Arial, sans-serif; background-color: #f4f4f4; padding: 20px;'>
                    <div style='background-color: white; padding: 30px; border-radius: 10px; max-width: 600px; margin: 0 auto;'>
                        <h2 style='color: #333;'>¡Hola {firstName}!</h2>
                        <p style='color: #666; font-size: 16px;'>Has solicitado un código de verificación de dos factores para tu cuenta.</p>
                        <div style='background-color: #f0f0f0; padding: 20px; border-radius: 5px; text-align: center; margin: 20px 0;'>
                            <p style='margin: 0; font-size: 32px; font-weight: bold; letter-spacing: 5px; color: #4CAF50;'>{code}</p>
                        </div>
                        <p style='color: #666; font-size: 14px;'>Este código expirará en 10 minutos.</p>
                        <p style='color: #999; font-size: 12px; margin-top: 20px;'>Si no solicitaste este código, puedes ignorar este email.</p>
                        <hr style='border: none; border-top: 1px solid #eee; margin: 20px 0;'>
                        <p style='color: #999; font-size: 12px;'>Saludos,<br>El equipo de AuthService</p>
                    </div>
                </body>
                </html>";

            await SendEmailAsync(email, subject, body);
        }

        private async Task SendEmailAsync(string toEmail, string subject, string body)
        {
            try
            {
                // Variable de entorno para redirección de email en pruebas
                var testEmail = Environment.GetEnvironmentVariable("TEST_EMAIL");
                var actualToEmail = !string.IsNullOrEmpty(testEmail) ? testEmail : toEmail;
                
                // Si estamos redirigiendo, agregar nota en el cuerpo del email
                if (!string.IsNullOrEmpty(testEmail) && testEmail != toEmail)
                {
                    body += $"<p style='color: red; font-size: 12px;'>[PRUEBA] Este email estaba destinado a: {toEmail}</p>";
                }

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

                mailMessage.To.Add(actualToEmail);

                await client.SendMailAsync(mailMessage);
                _logger.LogInformation("Email enviado exitosamente a {Email}", actualToEmail);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al enviar email a {Email}", toEmail);
                throw;
            }
        }
    }
}