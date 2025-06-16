using AuthService.Core.Enums;

namespace AuthService.Core.Models.Token
{
    public class TokenParameters
    {
       public string UserName { get; set; }
       public string FirstName { get; set; }
       public string LastName { get; set; }
       public string Id { get; set; }
       public TypeAuth TypeAuth { get; set; }
       public bool TFA { get; set; } = false;
    }
}
