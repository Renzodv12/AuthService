using AuthService.Core.Models.Security;
using MediatR;

namespace AuthService.Core.Feature.Commands.Security
{
    public record RevokeTokenCommand(string Jti, DateTime Expiry) : IRequest<RevokeToken>;
}
