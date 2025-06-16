using AuthService.Core.Entities;
using AuthService.Core.Feature.Querys.Security;
using MediatR;
using System.IdentityModel.Tokens.Jwt;
using System.Text.Json;

namespace AuthService.WebApi.Middleware
{
    public class JwtRevocationMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IMediator _mediator;

        public JwtRevocationMiddleware(RequestDelegate next, IMediator mediator)
        {
            _next = next;
            _mediator = mediator;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var token = context.Request.Headers.Authorization
                .FirstOrDefault()?.Split(" ").Last();

            if (!string.IsNullOrEmpty(token))
            {
                var handler = new JwtSecurityTokenHandler();
                if (handler.CanReadToken(token))
                {
                    var jwtToken = handler.ReadJwtToken(token);
                    var jti = jwtToken.Claims.FirstOrDefault(x => x.Type == JwtRegisteredClaimNames.Jti)?.Value;

                    if (!string.IsNullOrEmpty(jti))
                    {
                        var isRevoked = await _mediator.Send(new IsTokenRevokedQuery(jti));
                        if (isRevoked)
                        {
                            context.Response.StatusCode = 401;
                            await context.Response.WriteAsync(JsonSerializer.Serialize(new
                            {
                                error = "Token revocado",
                                message = "El token ha sido revocado y ya no es valido"
                            }));
                            return;
                        }
                    }
                    var has2fa = context.User.HasClaim("2fa", "true");

                    // Lista blanca: solo deja pasar si está en rutas permitidas sin 2FA
                    var path = context.Request.Path.Value?.ToLower();
                    var allowedWithout2FA = new[] { "/auth", "/Security/2FA" };

                    if (!has2fa && !allowedWithout2FA.Any(p => path.StartsWith(p)))
                    {
                        context.Response.StatusCode = StatusCodes.Status403Forbidden;
                        await context.Response.WriteAsync("Segundo factor Requerido.");
                        return;
                    }
                }
            }

            await _next(context);
        }
    }

}
