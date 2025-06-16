using AuthService.Core.Entities;
using AuthService.Core.Exceptions;
using AuthService.Core.Feature.Commands.Security;
using AuthService.Core.Feature.Commands.User;
using AuthService.Core.Feature.Querys.Security;
using AuthService.Core.Interfaces;
using AuthService.Core.Models.Security;
using AuthService.Core.Models.User;
using AuthService.WebApi.Extensions;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
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
               .WithOpenApi()
               .Accepts<Login>("application/json")
               .Produces<string>(StatusCodes.Status200OK)
               .Produces(StatusCodes.Status401Unauthorized);

            app.MapPost("/auth/Register", Register)
               .WithName("Register")
               .WithTags("Auth")
               .WithOpenApi()
               .Accepts<Register>("application/json")
               .Produces<string>(StatusCodes.Status200OK)
               .Produces(StatusCodes.Status401Unauthorized);

            app.MapPost("/auth/revoke", Revoke)
                .RequireAuthorization()
                .WithName("RevokeToken")
                .WithSummary("Revoca el token JWT actual")
                .WithTags("Auth")
                .WithOpenApi();

            app.MapPost("/auth/logout", Logout)
                .RequireAuthorization()
                .WithName("Logout")
                .WithSummary("Revoca el token JWT actual")
                .WithTags("Auth")
                .WithOpenApi();

            app.MapPost("/auth/token-status", TokenStatus)
             .RequireAuthorization()
             .WithName("TokenStatus")
             .WithSummary("Revoca el token JWT actual")
             .WithTags("Auth")
             .WithOpenApi();

            return app;
        }

        private static async Task<IResult> Login(Login request, IMediator _mediator)
        {
            try
            {
                var model = await _mediator.Send(new LoginCommand()
                {
                    login = request
                });
                return Results.Ok(model);

            }
            catch (UserOPasswordException)
            {
                return Results.Unauthorized();

            }catch (Exception)
            {
                return Results.BadRequest();
            }
        }

        private static async Task<IResult> Register(Register request, IValidator<Register> validator, IMediator _mediator)
        {
            try
            {
                var validationResult = await validator.ValidateAsync(request);

                if (!validationResult.IsValid)
                {
                    return Results.BadRequest(validationResult.Errors.Select(e => new
                    {
                        Property = e.PropertyName,
                        Error = e.ErrorMessage
                    }));
                }
                await _mediator.Send(new RegisterCommand()
                {
                    register = request
                });
                return Results.Created();

            }
            catch (UserOPasswordException ex)
            {
                return Results.BadRequest(new { Error= ex.Message});

            }
            catch (Exception ex)
            {
                return Results.BadRequest(new { Error = ex.Message });
            }
        }
    
        private static async Task<IResult> Revoke(HttpContext context, IMediator mediator) 
        {
                 var(jti, userId, expiry, email) = context.ExtractTokenClaims();

                 if (string.IsNullOrEmpty(jti) || !expiry.HasValue)
                 {
                     return Results.BadRequest(new { error = "Token inválido o sin JTI" });
                 }

            var result = await mediator.Send(new RevokeTokenCommand(jti, expiry.Value));

            return result.Success
                ? Results.Ok(new RevokeTokenResponse(true, result.Message))
                : Results.BadRequest(new RevokeTokenResponse(false, result.Message));
             }
    
        private static async Task<IResult> Logout(HttpContext context, IMediator mediator)
        {
            var (jti, userId, expiry, email) = context.ExtractTokenClaims();

            if (string.IsNullOrEmpty(jti) || !expiry.HasValue)
            {
                return Results.BadRequest(new { error = "Token inválido" });
            }

            var result = await mediator.Send(new RevokeTokenCommand(jti, expiry.Value));

            return Results.Ok(new
            {
                message = "Logout exitoso",
                success = result.Success,
                timestamp = DateTime.UtcNow
            });


        }

        private static async Task<IResult> TokenStatus(HttpContext context, IMediator mediator)
        {
            var (jti, userId, expiry, email) = context.ExtractTokenClaims();

            if (string.IsNullOrEmpty(jti))
            {
                return Results.BadRequest(new { error = "Token sin JTI" });
            }

            var isRevoked = await mediator.Send(new IsTokenRevokedQuery(jti));

            return Results.Ok(new
            {
                jti,
                userId,
                expiry,
                isRevoked,
                isActive = !isRevoked,
                timestamp = DateTime.UtcNow
            });

        }
    }
}
