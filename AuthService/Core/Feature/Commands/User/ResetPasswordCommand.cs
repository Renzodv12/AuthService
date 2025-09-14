using MediatR;

namespace AuthService.Core.Feature.Commands.User
{
    public class ResetPasswordCommand : IRequest<bool>
    {
        public string Token { get; set; }
        public string Email { get; set; }
        public string NewPassword { get; set; }
    }
}