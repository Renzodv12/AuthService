namespace AuthService.Core.Entities
{
    public class EmailVerificationCode
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid UserId { get; set; }
        public string Code { get; set; }
        public string Email { get; set; }
        public DateTime ExpiresAt { get; set; }
        public bool IsUsed { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UsedAt { get; set; }
        
        // Navigation property
        public User User { get; set; }
        
        public bool IsExpired => DateTime.UtcNow > ExpiresAt;
        public bool IsValid => !IsUsed && !IsExpired;
    }
}

