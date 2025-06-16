using AuthService.Core.Entities;
using AuthService.Core.Enums;
using AuthService.Core.Exceptions;
using AuthService.Core.Feature.Commands.Security;
using AuthService.Core.Feature.Querys.Security;
using AuthService.Core.Interfaces;
using AuthService.Core.Models.Security;
using MediatR;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Security.Claims;

namespace AuthService.Core.Feature.Handler.Security
{
    public class MethodsAllowedForUserHandler : IRequestHandler<MethodsAllowedForUserQuery, List<MethodsAllowedForUser>>
    {
        private readonly IRepository<UserAuthenticationMethods> _userAuthenticationMethodsRepository;
        private readonly ILogger<MethodsAllowedForUserHandler> _logger;
        private readonly IHttpContextAccessor _httpContext;

        public MethodsAllowedForUserHandler(IRepository<UserAuthenticationMethods> userAuthenticationMethodsRepository, ILogger<MethodsAllowedForUserHandler> logger, IHttpContextAccessor httpContext)
        {
            _userAuthenticationMethodsRepository = userAuthenticationMethodsRepository;
            _logger = logger;
            _httpContext = httpContext; 
        }

        public async Task<List<MethodsAllowedForUser>> Handle(MethodsAllowedForUserQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var userId = _httpContext?.HttpContext?.User.FindFirstValue("Id");
                var methods = await _userAuthenticationMethodsRepository.WhereAsync(x => x.IdUser == Guid.Parse(userId));
                var result = (from m in methods
                             select new MethodsAllowedForUser() {
                             Id = (int)m.TypeAuth,
                             Name = m.TypeAuth.ToString(),
                             Enabled = m.Enabled
                             }).ToList();
                result.Add(new MethodsAllowedForUser()
                {
                    Id = (int)TypeAuth.Password,
                    Name = TypeAuth.Password.ToString(),
                    Enabled =true
                });


                return result.OrderBy(x=>x.Id).ToList();


            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ocurrio un error al obtener los metodos de seguridad habilitados");
                throw new DefaultException("Ocurrio un error al obtener los metodos de seguridad habilitados");
            }
        }

    }
}
