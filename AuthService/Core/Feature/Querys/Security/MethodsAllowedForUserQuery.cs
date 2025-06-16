using AuthService.Core.Models.Security;
using MediatR;

namespace AuthService.Core.Feature.Querys.Security
{
    public record MethodsAllowedForUserQuery() : IRequest<List<MethodsAllowedForUser>>;
}
