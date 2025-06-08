using AuthService.Core.Interfaces;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using System.Security.Cryptography.X509Certificates;

namespace AuthService.WebApi.Endpoints
{
    public static class AuthEndpoints
    {
        public static IEndpointRouteBuilder MapAuthEndpoints(this IEndpointRouteBuilder app)
        {
            app.MapPost("/auth/login", Login)
               .WithName("Login")
               .WithTags("Auth")
             //  .Accepts<LoginRequest>("application/json")
               .Produces<string>(StatusCodes.Status200OK)
               .Produces(StatusCodes.Status401Unauthorized);

            return app;
        }

        private static IResult Login(string request, IToken tokenService)
        {
            var result = tokenService.GenerateJwtToken(new Core.Models.Token.TokenParameters()
            {
                Id = request,
                UserName = request,
                PasswordHash = request
            });

            if (result == null)
                return Results.Unauthorized();

            return Results.Ok(result);
        }
    }
}
