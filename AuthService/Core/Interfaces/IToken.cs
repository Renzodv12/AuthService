using AuthService.Core.Models.Token;

namespace AuthService.Core.Interfaces
{
    public interface IToken
    {
        string GenerateJwtToken(TokenParameters pars);

    }
}
