using AuthService.Core.Enums;
using System.ComponentModel.DataAnnotations;

namespace AuthService.Core.Models.User
{
    public class Register
    {
        [Required]
        public string FirstName { get; set; }
        [Required]
        public string LastName { get; set; }
        [Required]
        [EmailAddress]
        public string Email { get; set; }
        [Required]
        [MinLength(6)]
        public string Password { get; set; }
        [Required]
        [MinLength(6)]
        public string ConfirmPassword { get; set; }
        [Required]
        [MinLength(4)]
        [MaxLength(20)]
        public string CI { get; set; }
        [Required]
        public DateTime BirthDate { get; set; }
    }
}
