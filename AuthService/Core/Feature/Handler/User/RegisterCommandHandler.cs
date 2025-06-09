using AuthService.Core.Feature.Commands.User;
using AuthService.Core.Interfaces;
using MediatR;
using AuthService.Core.Entities;
using AuthService.Core.Utils;
using AuthService.Core.Exceptions;
namespace AuthService.Core.Feature.Handler.User
{
    public class RegisterCommandHandler : IRequestHandler<RegisterCommand>
    {
        private readonly IRepository<AuthService.Core.Entities.User> _userRepository;
        public RegisterCommandHandler(IRepository<AuthService.Core.Entities.User> userRepository)
        {
            _userRepository = userRepository;
        }
        public async Task Handle(RegisterCommand request, CancellationToken cancellationToken)
        {
            var verifyUsuario = await _userRepository.FirstOrDefaultAsync(x => x.Email == request.register.Email || x.CI == request.register.CI);
            if(verifyUsuario == null) {
                var salt = PasswordHelper.GenerateSalt();
                var hashedPassword = PasswordHelper.HashPassword(request.register.Password, salt);
                var user = new AuthService.Core.Entities.User
                {
                    Id = Guid.NewGuid(),
                    FirstName = request.register.FirstName,
                    LastName = request.register.LastName,
                    Email = request.register.Email,
                    Password = hashedPassword,
                    Salt = salt,
                    CI = request.register.CI,
                    BirthDate = request.register.BirthDate,
                    TypeAuth = Enums.TypeAuth.Password,
                    CreateDate = DateTime.UtcNow,
                    LastModifiedDate = DateTime.UtcNow
                };

                await _userRepository.AddAsync(user);
                await _userRepository.SaveChangesAsync();
            }
            else
            {
                throw new UserAlreadyExistsException(request.register.Email, request.register.CI);
            }
        }
    }
}
