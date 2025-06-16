using AuthService.Core.Feature.Commands.Security;
using AuthService.Core.Models.Security;
using MediatR;
using StackExchange.Redis;
using System.Text.Json;

namespace AuthService.Core.Feature.Handler.Security
{
    public class RevokeTokenHandler : IRequestHandler<RevokeTokenCommand, RevokeToken>
    {
        private readonly IDatabase _database;
        private readonly ILogger<RevokeTokenHandler> _logger;

        public RevokeTokenHandler(IConnectionMultiplexer redis, ILogger<RevokeTokenHandler> logger)
        {
            _database = redis.GetDatabase();
            _logger = logger;
        }

        public async Task<RevokeToken> Handle(RevokeTokenCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var key = $"revoked_token:{request.Jti}";
                var expiry = request.Expiry.Subtract(DateTime.UtcNow);

                if (expiry <= TimeSpan.Zero)
                {
                    return new RevokeToken(false, "Token ya expirado");
                }

                await _database.StringSetAsync(key, JsonSerializer.Serialize(new
                {
                    RevokedAt = DateTime.UtcNow,
                    ExpiresAt = request.Expiry
                }), expiry);

                _logger.LogInformation("Token {Jti} revocado exitosamente", request.Jti);
                return new RevokeToken(true, "Token revocado exitosamente");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al revocar token {Jti}", request.Jti);
                return new RevokeToken(false, "Error al revocar token");
            }
        }
    }
}
