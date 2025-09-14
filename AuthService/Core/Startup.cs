using AuthService.Core.Interfaces;
using AuthService.Core.Services;
using AuthService.Infrastructure.Configuration;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using FluentValidation;
using AuthService.Core.Entities;
using Microsoft.AspNetCore.Identity;
using AuthService.Core.Models.User;
using AuthService.Core.Validators.User;
using System.Reflection;
using Google.Authenticator;
using Hangfire;
using Hangfire.PostgreSql;
using AuthService.Core.Models.Email;

namespace AuthService.Core
{
    public static class Startup
    {
        public static IServiceCollection AddCoreServices(this IServiceCollection services,  IConfiguration configuration)
        {
            services.Configure<JwtConfig>(configuration.GetSection("JwtConfig"));
            var key = Encoding.ASCII.GetBytes(configuration["JwtConfig:Secret"]);

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo { Title = "AuthService", Version = "v1" });

                // Agrega la autorización en Swagger
                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Description = "JWT Authorization header using the Bearer scheme.",
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.Http,
                    Scheme = "Bearer"
                });

                c.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        },
                        new string[] {}
                    }
                });
            });

            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ValidateLifetime = false,
                    ValidateIssuerSigningKey = false,
                    ValidIssuer = null,
                    ValidAudience = null,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                };
            });

            services.AddAuthorization();
            services.AddScoped(typeof(IToken), typeof(TokenService));
            services.AddScoped<IRedisService, RedisService>();
            services.AddValidatorsFromAssemblyContaining<RegisterValidator>();
            services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));
            services.AddTransient<TwoFactorAuthenticator>();
            services.AddHttpContextAccessor();

            // Email Services
            services.Configure<EmailSettings>(configuration.GetSection("EmailSettings"));
            services.AddScoped<IEmailService, EmailService>();
            services.AddScoped<IEmailBackgroundService, EmailBackgroundService>();

            // Hangfire Configuration
            services.AddHangfire(config => config
                .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
                .UseSimpleAssemblyNameTypeSerializer()
                .UseRecommendedSerializerSettings()
                .UsePostgreSqlStorage(configuration.GetConnectionString("DefaultConnection")));

            services.AddHangfireServer();

            return services;
        }
    }
 }
