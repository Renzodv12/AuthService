using AuthService.Core.Enums;

namespace AuthService.Core.Entities
{
    public class UserAuthenticationMethods : OwnedByUserEntity
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public TypeAuth TypeAuth { get; set; }
        public bool Enabled { get; set; }
        public string Key { get; set; }
    }
}
