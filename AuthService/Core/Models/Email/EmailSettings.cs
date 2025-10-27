namespace AuthService.Core.Models.Email
{
    public class EmailSettings
    {
        public string SmtpServer { get; set; }
        public int SmtpPort { get; set; }
        public string SmtpUsername { get; set; }
        public string SmtpPassword { get; set; }
        public bool EnableSsl { get; set; }
        public string FromEmail { get; set; }
        public string FromName { get; set; }
        public string VerificationEmailTemplate { get; set; }
        public string PasswordResetTemplate { get; set; }
        public string WelcomeEmailTemplate { get; set; }
        public string BaseUrl { get; set; }
        public string FrontendBaseUrl { get; set; }
    }
}