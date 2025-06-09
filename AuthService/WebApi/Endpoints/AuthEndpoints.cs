using AuthService.Core.Exceptions;
using AuthService.Core.Feature.Commands.User;
using AuthService.Core.Interfaces;
using AuthService.Core.Models.User;
using MediatR;
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
               .Accepts<Login>("application/json")
               .Produces<string>(StatusCodes.Status200OK)
               .Produces(StatusCodes.Status401Unauthorized);

            app.MapPost("/auth/Register", Register)
               .WithName("Register")
               .WithTags("Auth")
               .Accepts<Register>("application/json")
               .Produces<string>(StatusCodes.Status200OK)
               .Produces(StatusCodes.Status401Unauthorized);

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

        private static async Task<IResult> Register(Register request, IMediator _mediator)
        {
            try
            {
                await _mediator.Send(new RegisterCommand()
                {
                    register = request
                });
                return Results.Created();

            }
            catch (UserOPasswordException)
            {
                return Results.BadRequest();

            }
            catch (Exception)
            {
                return Results.BadRequest();
            }
        }
    }
}
