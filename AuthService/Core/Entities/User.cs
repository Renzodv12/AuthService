using AuthService.Core.Enums;
using System.ComponentModel.DataAnnotations;

namespace AuthService.Core.Entities
{
    public class User
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string Salt { get; set; }
        public string CI { get; set; }
        public DateTime BirthDate { get; set; }
        public TypeAuth TypeAuth { get; set; }
        public DateTime CreateDate { get; set; }
        public DateTime LastModifiedDate { get; set; }

    }
}
