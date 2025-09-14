using MediatR;

namespace AuthService.Core.Feature.Commands.User
{
    public class RequestPasswordResetCommand : IRequest<bool>
    {
        public string Email { get; set; }
    }
}