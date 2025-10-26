using AuthService.Core.Feature.Commands.User;
using AuthService.Core.Interfaces;
using MediatR;
using AuthService.Core.Entities;
using AuthService.Core.Utils;
using AuthService.Core.Exceptions;
using AuthService.Infrastructure.Context;
namespace AuthService.Core.Feature.Handler.User
{
    public class LoginCommandHandler : IRequestHandler<LoginCommand, string>
    {
        private readonly IRepository<AuthService.Core.Entities.User> _userRepository;
        private readonly IToken _token;
        public LoginCommandHandler(IRepository<AuthService.Core.Entities.User> userRepository, IToken token)
        {
            _userRepository = userRepository;
            _token = token;
        }
        public async Task<string> Handle(LoginCommand request, CancellationToken cancellationToken)
        {
            var user = await _userRepository.FirstOrDefaultAsync(x => x.Email == request.login.Email );
            if(user != null) {
               if (PasswordHelper.VerifyPassword(request.login.Password, user.Password, user.Salt) || request.TFA)
                {
                    return _token.GenerateJwtToken(new Models.Token.TokenParameters()
                    {
                        Id = user.Id.ToString(),
                        UserName = user.Email,
                        TypeAuth = user.TypeAuth,
                         TFA = user.TypeAuth != Enums.TypeAuth.Password ? request.TFA : true,
                        //  TFA =  true,
                        FirstName = user.FirstName,
                        LastName = user.LastName,
                        
                    });
                }
                else
                {
                     throw new UserOPasswordException();
                }
            }
            else
            {
                throw new UserOPasswordException();
            }
        }
    }
}
