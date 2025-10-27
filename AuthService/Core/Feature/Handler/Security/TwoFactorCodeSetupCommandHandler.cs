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
using Hangfire;

namespace AuthService.Core.Feature.Handler.Security
{
    public class TwoFactorCodeSetupCommandHandler : IRequestHandler<TwoFactorCodeSetupCommand, TwoFactorCodeSetup>
    {
        private readonly IRepository<AuthService.Core.Entities.User> _userRepository;
        private readonly IRepository<AuthService.Core.Entities.UserAuthenticationMethods> _userAuthenticationMethodsRepository;
        private readonly IRepository<EmailVerificationCode> _emailCodeRepository;
        private readonly ILogger<TwoFactorCodeSetupCommandHandler> _logger;
        private readonly TwoFactorAuthenticator _twoFactorAuthentication;
        private readonly IHttpContextAccessor _httpContext;
        private readonly IEmailBackgroundService _emailBackgroundService;
        public TwoFactorCodeSetupCommandHandler(IRepository<AuthService.Core.Entities.User> userRepository,
                                                IRepository<AuthService.Core.Entities.UserAuthenticationMethods> userAuthenticationMethodsRepository,
                                                IRepository<EmailVerificationCode> emailCodeRepository,
                                                ILogger<TwoFactorCodeSetupCommandHandler> logger,
                                                TwoFactorAuthenticator twoFactorAuthentication,
                                                IHttpContextAccessor httpContext,
                                                IEmailBackgroundService emailBackgroundService)
        {
            _userRepository = userRepository;
            _userAuthenticationMethodsRepository = userAuthenticationMethodsRepository;
            _emailCodeRepository = emailCodeRepository;
            _logger = logger;
            _twoFactorAuthentication = twoFactorAuthentication;
            _httpContext = httpContext;
            _emailBackgroundService = emailBackgroundService;
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
                        byte[] key = new byte[20];
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
                        // Invalidar códigos anteriores
                        var existingCodes = (await _emailCodeRepository.GetAllAsync())
                            .Where(x => x.UserId == customer.Id && !x.IsUsed && x.ExpiresAt > DateTime.UtcNow);
                        
                        foreach (var existingCode in existingCodes)
                        {
                            existingCode.IsUsed = true;
                            existingCode.UsedAt = DateTime.UtcNow;
                        }
                        await _emailCodeRepository.SaveChangesAsync();

                        // Generar código de 6 dígitos
                        var random = new Random();
                        var code = random.Next(100000, 999999).ToString();
                        
                        // Guardar código en la base de datos
                        var emailCode = new EmailVerificationCode
                        {
                            Id = Guid.NewGuid(),
                            UserId = customer.Id,
                            Code = code,
                            Email = customer.Email,
                            ExpiresAt = DateTime.UtcNow.AddMinutes(10), // 10 minutos de validez
                            IsUsed = false,
                            CreatedAt = DateTime.UtcNow
                        };
                        
                        await _emailCodeRepository.AddAsync(emailCode);
                        await _emailCodeRepository.SaveChangesAsync();
                        
                        // Enviar código por email usando Hangfire
                        Hangfire.BackgroundJob.Enqueue(() => _emailBackgroundService.SendTwoFactorCodeAsync(
                            customer.Email, customer.FirstName, code));
                        
                        method.Key = code; // Guardar el código como Key para referencia
                        method.Enabled = false; // No habilitar hasta que el usuario verifique el código
                        model.CustomValues.Add("CodeSent", "true");
                        model.CustomValues.Add("CodeExpirationMinutes", "10");
                        _logger.LogInformation("Código de 2FA por email generado para usuario {Email}", customer.Email);
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
