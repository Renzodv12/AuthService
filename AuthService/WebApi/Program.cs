using AuthService.Core;
using AuthService.Infrastructure;
using AuthService.WebApi.Endpoints;
using AuthService.WebApi.Middleware;
using Microsoft.Extensions.Configuration;
using Hangfire;
using Serilog;

try
{
    var builder = WebApplication.CreateBuilder(args);
    
    // Configure Serilog
    Log.Logger = new LoggerConfiguration()
        .ReadFrom.Configuration(builder.Configuration)
        .CreateLogger();
    
    Log.Information("Starting AuthService application");
    
    // Add Serilog
    builder.Host.UseSerilog();

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddCoreServices(builder.Configuration);
builder.Services.AddInfrastructureServices(builder.Configuration);


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}



app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.UseMiddleware<JwtMiddleware>();

// Hangfire Dashboard
if (app.Environment.IsDevelopment())
{
    app.UseHangfireDashboard("/hangfire");
}

app.MapAuthEndpoints();
app.MapSecurityEndpoints();

    Log.Information("AuthService application started successfully");
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "AuthService application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}
