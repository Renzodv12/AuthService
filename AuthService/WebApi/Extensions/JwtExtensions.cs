using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace AuthService.WebApi.Extensions
{
    public static class JwtExtensions
    {
        public static (string? Jti, string? UserId, DateTime? Expiry, string? email) ExtractTokenClaims(this HttpContext context)
        {
            var jti = context.User.FindFirst(JwtRegisteredClaimNames.Jti)?.Value;
            var userId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var expClaim = context.User.FindFirst(JwtRegisteredClaimNames.Exp)?.Value;
            var email = context.User.FindFirst(JwtRegisteredClaimNames.Email)?.Value;

            DateTime? expiry = null;
            if (!string.IsNullOrEmpty(expClaim) && long.TryParse(expClaim, out var exp))
            {
                expiry = DateTimeOffset.FromUnixTimeSeconds(exp).DateTime;
            }

            return (jti, userId, expiry, email);
        }
    }
}
