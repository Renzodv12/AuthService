namespace AuthService.Core.Interfaces
{
    public interface IEmailBackgroundService
    {
        Task SendEmailVerificationAsync(string email, string firstName, string verificationToken);
        Task SendPasswordResetAsync(string email, string firstName, string resetToken);
        Task SendWelcomeEmailAsync(string email, string firstName);
        Task SendTwoFactorCodeAsync(string email, string firstName, string code);
    }
}