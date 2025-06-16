using AuthService.Core.Feature.Querys.Security;
using MediatR;
using StackExchange.Redis;
using System.IdentityModel.Tokens.Jwt;

namespace AuthService.Core.Feature.Handler.Security
{
    public class IsTokenRevokedHandler : IRequestHandler<IsTokenRevokedQuery, bool>
    {
        private readonly IDatabase _database;
        private readonly ILogger<IsTokenRevokedHandler> _logger;

        public IsTokenRevokedHandler(IConnectionMultiplexer redis, ILogger<IsTokenRevokedHandler> logger)
        {
            _database = redis.GetDatabase();
            _logger = logger;
        }

        public async Task<bool> Handle(IsTokenRevokedQuery request, CancellationToken cancellationToken)
        {
            try
            {
                // Verificar si el token específico está revocado
                var tokenKey = $"revoked_token:{request.Jti}";
                var tokenExists = await _database.KeyExistsAsync(tokenKey);

                if (tokenExists)
                {
                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al verificar si token {Jti} está revocado", request.Jti);
                return false;
            }
        }
    }
}
