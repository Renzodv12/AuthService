using AuthService.Core.Enums;
using AuthService.Core.Models.Security;
using MediatR;

namespace AuthService.Core.Feature.Commands.Security
{
    public class TwoFactorCodeSetupCommand : IRequest<TwoFactorCodeSetup>
    {
        public string? email { get; set; }
        public TypeAuth typeAuth { get; set; }
    }
}
