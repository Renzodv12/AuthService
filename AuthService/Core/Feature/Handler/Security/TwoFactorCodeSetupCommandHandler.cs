using AuthService.Core.Entities;
using AuthService.Core.Enums;
using AuthService.Core.Exceptions;
using AuthService.Core.Feature.Commands.Security;
using AuthService.Core.Feature.Commands.User;
using AuthService.Core.Interfaces;
using AuthService.Core.Models.Security;
using Google.Authenticator;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Security.Claims;
using System.Security.Cryptography;

namespace AuthService.Core.Feature.Handler.Security
{
    public class TwoFactorCodeSetupCommandHandler : IRequestHandler<TwoFactorCodeSetupCommand, TwoFactorCodeSetup>
    {
        private readonly IRepository<AuthService.Core.Entities.User> _userRepository;
        private readonly IRepository<AuthService.Core.Entities.UserAuthenticationMethods> _userAuthenticationMethodsRepository;
        private readonly ILogger<TwoFactorCodeSetupCommandHandler> _logger;
        private readonly TwoFactorAuthenticator _twoFactorAuthentication;
        private readonly IHttpContextAccessor _httpContext;
        public TwoFactorCodeSetupCommandHandler(IRepository<AuthService.Core.Entities.User> userRepository,
                                                IRepository<AuthService.Core.Entities.UserAuthenticationMethods> userAuthenticationMethodsRepository,
                                                ILogger<TwoFactorCodeSetupCommandHandler> logger,
                                                TwoFactorAuthenticator twoFactorAuthentication,
                                                IHttpContextAccessor httpContext)
        {
            _userRepository = userRepository;
            _userAuthenticationMethodsRepository = userAuthenticationMethodsRepository;
            _logger = logger;
            _twoFactorAuthentication = twoFactorAuthentication;
            _httpContext = httpContext;
        }
        public async Task<TwoFactorCodeSetup> Handle(TwoFactorCodeSetupCommand request,  CancellationToken cancellationToken)
        {
            try
            {
                request.email = _httpContext?.HttpContext?.User.FindFirstValue(ClaimTypes.Email);
                if (request.email == null)
                    throw new DefaultException($"No se encontro el usuario ${request.email}");
                var model = new TwoFactorCodeSetup();
                var customer = await _userRepository.FirstOrDefaultAsync(x => x.Email == request.email);
                if (customer == null)
                    throw new DefaultException($"No se encontro el usuario ${request.email}");

                var method = await GetOrdAddAuthenticationMethod(customer.Id, request.typeAuth);
                _logger.LogInformation("Inicio de Peticion de codigo de 2FA {Request}", request);
                switch (request.typeAuth)
                {
                    case TypeAuth.GoogleAuthenticator:
                        byte[] key = new byte[6];
                        var rng = RandomNumberGenerator.Create();
                        rng.GetBytes(key);
                        string secretKey = Convert.ToBase64String(key);
                        var setupInfo = _twoFactorAuthentication.GenerateSetupCode("ERP", customer.Email, secretKey, false);
                        model.CustomValues.Add("QrCodeImageUrl", setupInfo.QrCodeSetupImageUrl);
                        model.CustomValues.Add("ManualEntryQrCode", setupInfo.ManualEntryKey);
                        method.Key = secretKey;
                        method.Enabled = true;
                        await _userRepository.SaveChangesAsync();
                        break;

                    case TypeAuth.EmailVerificationCode:
                        throw new NotImplementedException();
                     break;
                }
                return model;
            }catch(DefaultException ex)
            {
                _logger.LogWarning(ex, ex.Message);

                throw new DefaultException(ex.Message);

            }catch(NotImplementedException ex)
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

        private async Task<UserAuthenticationMethods> GetOrdAddAuthenticationMethod(Guid idUser, TypeAuth typeauth)
        {
            var method = await _userAuthenticationMethodsRepository.FirstOrDefaultAsync(x => x.IdUser == idUser && x.TypeAuth == typeauth);
            if (method == null)
            {
                method = new UserAuthenticationMethods() { TypeAuth = typeauth, IdUser = idUser, Enabled =false};
                 await _userAuthenticationMethodsRepository.AddAsync(method);
            }
            return method;
        }
    }
}
