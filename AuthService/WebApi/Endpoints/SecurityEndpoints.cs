using AuthService.Core.Entities;
using AuthService.Core.Enums;
using AuthService.Core.Feature.Commands.Security;
using AuthService.Core.Feature.Commands.User;
using AuthService.Core.Models.Security;
using AuthService.Core.Models.User;
using AuthService.WebApi.Extensions;
using Google.Authenticator;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System;
using System.Security.Claims;
using System.Security.Cryptography;

namespace AuthService.WebApi.Endpoints
{
    public static class SecurityEndpoints
    {
  
        public static IEndpointRouteBuilder MapSecurityEndpoints(this IEndpointRouteBuilder app)
        {
            app.MapGet("/Security/2FA", TFA)
              .WithName("Security")
              .WithTags("Security")
              .WithOpenApi()
              .RequireAuthorization()
              .Produces<TwoFactorCodeSetup>(StatusCodes.Status200OK)
              .Produces(StatusCodes.Status401Unauthorized);

            app.MapPost("/Security/2FA", TFAConfirm)
                 .WithName("SecurityConfirm")
                 .WithTags("Security")
                 .WithOpenApi()
                 .RequireAuthorization()
                 .Accepts<AuthenticateTwoFactorCommand>("application/json")
                 .Produces(StatusCodes.Status202Accepted)
                 .Produces(StatusCodes.Status401Unauthorized);
            return app;
        }
        private static async Task<IResult> TFA(TypeAuth typeAuth, IMediator _mediator)
        {
            try
            {
                var result = await _mediator.Send(new TwoFactorCodeSetupCommand()
                {
                    email = null,
                    typeAuth = typeAuth
                });
                return Results.Ok(result);

            }catch(Exception ex)
            {
                return Results.BadRequest(new { Error = ex.Message });

            }



        }

        private static async Task<IResult> TFAConfirm(AuthenticateTwoFactorCommand request, IMediator _mediator, HttpContext context)
        {
            try
            {
                var result = await _mediator.Send(request);
                if (result)
                {
                    var model = await _mediator.Send(new LoginCommand()
                    {
                        login = new Login()
                        {
                            Email = context.ExtractTokenClaims().email ?? "",
                            Password = "",
                        },
                        TFA = true

                    });
                    return Results.Ok(model);
                }
                else
                {
                    return Results.BadRequest();
                }

            }
            catch (Exception ex)
            {
                return Results.BadRequest(new { Error = ex.Message });

            }
        }

      
    }
}
