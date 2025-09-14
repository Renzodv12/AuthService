using MediatR;

namespace AuthService.Core.Feature.Commands.User
{
    public class VerifyEmailCommand : IRequest<bool>
    {
        public string Token { get; set; }
        public string Email { get; set; }
    }
}