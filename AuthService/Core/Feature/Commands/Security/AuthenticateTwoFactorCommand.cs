using AuthService.Core.Enums;
using MediatR;

namespace AuthService.Core.Feature.Commands.Security
{
    public class AuthenticateTwoFactorCommand : IRequest<bool>
    {
        public string secretKey { get; set; }
        public TypeAuth typeAuth { get; set; }

    }
}
