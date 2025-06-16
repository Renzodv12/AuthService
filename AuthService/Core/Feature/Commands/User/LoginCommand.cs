using AuthService.Core.Enums;
using AuthService.Core.Models.User;
using MediatR;

namespace AuthService.Core.Feature.Commands.User
{
    public class LoginCommand : IRequest<string>
    {
        public Login login { get; set; }        
        public bool TFA { get; set; }        
    }
}
