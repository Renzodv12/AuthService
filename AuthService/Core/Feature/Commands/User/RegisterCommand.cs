using AuthService.Core.Enums;
using AuthService.Core.Models.User;
using MediatR;

namespace AuthService.Core.Feature.Commands.User
{
    public class RegisterCommand : IRequest
    {
        public Register register { get; set; }        
    }
}
