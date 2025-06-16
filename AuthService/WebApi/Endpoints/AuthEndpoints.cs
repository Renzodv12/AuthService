using AuthService.Core.Exceptions;
using AuthService.Core.Feature.Commands.User;
using AuthService.Core.Interfaces;
using AuthService.Core.Models.User;
using FluentValidation;
using MediatR;
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
    }
}
