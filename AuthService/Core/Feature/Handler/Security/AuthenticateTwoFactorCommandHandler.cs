using AuthService.Core.Entities;
using AuthService.Core.Enums;
using AuthService.Core.Exceptions;
using AuthService.Core.Feature.Commands.Security;
using AuthService.Core.Interfaces;
using AuthService.Core.Models.Security;
using Google.Authenticator;
using MediatR;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.Security.Claims;
using System.Security.Cryptography;

namespace AuthService.Core.Feature.Handler.Security
{
    public class AuthenticateTwoFactorCommandHandler : IRequestHandler<AuthenticateTwoFactorCommand, bool>
    {
        private readonly IRepository<AuthService.Core.Entities.User> _userRepository;
        private readonly IRepository<AuthService.Core.Entities.UserAuthenticationMethods> _userAuthenticationMethodsRepository;
        private readonly IRepository<EmailVerificationCode> _emailCodeRepository;
        private readonly ILogger<AuthenticateTwoFactorCommandHandler> _logger;
        private readonly TwoFactorAuthenticator _twoFactorAuthentication;
        private readonly IHttpContextAccessor _httpContext;
        public AuthenticateTwoFactorCommandHandler(IRepository<AuthService.Core.Entities.User> userRepository,
                                                IRepository<AuthService.Core.Entities.UserAuthenticationMethods> userAuthenticationMethodsRepository,
                                                IRepository<EmailVerificationCode> emailCodeRepository,
                                                ILogger<AuthenticateTwoFactorCommandHandler> logger,
                                                TwoFactorAuthenticator twoFactorAuthentication,
                                                IHttpContextAccessor httpContext)
        {
            _userRepository = userRepository;
            _userAuthenticationMethodsRepository = userAuthenticationMethodsRepository;
            _emailCodeRepository = emailCodeRepository;
            _logger = logger;
            _twoFactorAuthentication = twoFactorAuthentication;
            _httpContext = httpContext;
        }
        public async Task<bool> Handle(AuthenticateTwoFactorCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var email = _httpContext?.HttpContext?.User.FindFirstValue(ClaimTypes.Email);
                if (email == null)
                    throw new DefaultException($"No se encontro el usuario ${email}");
                bool result = false;
                var customer = await _userRepository.FirstOrDefaultAsync(x => x.Email == email);
                if (customer == null)
                    throw new DefaultException($"No se encontro el usuario ${email}");
                var method = await _userAuthenticationMethodsRepository.FirstOrDefaultAsync(x=> x.IdUser == customer.Id && x.TypeAuth == request.typeAuth);
                if(method == null)
                    throw new DefaultException($"No se configuro el method ${request.typeAuth.ToString()}");

                _logger.LogInformation("Inicio de Peticion de codigo de 2FA {Request}", request);
                switch (request.typeAuth)
                {
                    case TypeAuth.GoogleAuthenticator:
                         result = _twoFactorAuthentication.ValidateTwoFactorPIN(method.Key, request.secretKey, TimeSpan.FromSeconds(60));
                     
                        break;

                    case TypeAuth.EmailVerificationCode:
                        // Buscar código de verificación por email
                        var emailCode = await _emailCodeRepository.FirstOrDefaultAsync(
                            x => x.UserId == customer.Id && x.Code == request.secretKey && !x.IsUsed && x.Email == customer.Email);
                        
                        if (emailCode == null)
                        {
                            _logger.LogWarning("Código de verificación no válido para usuario {Email}", email);
                            return false;
                        }
                        
                        if (emailCode.IsExpired)
                        {
                            _logger.LogWarning("Código de verificación expirado para usuario {Email}", email);
                            return false;
                        }
                        
                        // Marcar código como usado
                        emailCode.IsUsed = true;
                        emailCode.UsedAt = DateTime.UtcNow;
                        await _emailCodeRepository.SaveChangesAsync();
                        
                        result = true;
                        _logger.LogInformation("Código de verificación validado exitosamente para usuario {Email}", email);
                        break;
                }

                if(result && customer.TypeAuth != request.typeAuth)
                {
                    customer.TypeAuth = request.typeAuth;
                    customer.LastModifiedDate = DateTime.UtcNow;
                    _userRepository.Update(customer);
                    await _userRepository.SaveChangesAsync();
                }
                if (result && !method.Enabled)
                {
                    method.Enabled = true;
                    _userAuthenticationMethodsRepository.Update(method);
                    await _userAuthenticationMethodsRepository.SaveChangesAsync();
                }
                return result;
            }
            catch (DefaultException ex)
            {
                _logger.LogWarning(ex, ex.Message);

                throw new DefaultException(ex.Message);

            }
            catch (NotImplementedException ex)
            {
                _logger.LogWarning(ex, ex.Message);
                throw new DefaultException("Opcion aun en Desarrollo o no Disponible");

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ocurrio un error al crear el codigo para el segundo factor");
                throw new DefaultException("Ocurrio un error al crear el codigo para el segundo factor");
            }
        }
    }
}
