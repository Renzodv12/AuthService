namespace AuthService.Core.Models.Token
{
    public class TokenParameters
    {
       public string UserName { get; set; }
       public string PasswordHash { get; set; }
       public string Id { get; set; }
    }
}
