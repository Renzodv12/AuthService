using MediatR;

namespace AuthService.Core.Feature.Querys.Security
{
    public record IsTokenRevokedQuery(string Jti) : IRequest<bool>;
}
