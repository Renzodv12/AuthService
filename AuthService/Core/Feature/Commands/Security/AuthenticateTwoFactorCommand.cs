using AuthService.Core.Enums;
using MediatR;
using System.Text.Json.Serialization;

namespace AuthService.Core.Feature.Commands.Security
{
    public class AuthenticateTwoFactorCommand : IRequest<bool>
    {
        public  string secretKey { get; set; }
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public TypeAuth typeAuth { get; set; }

    }
}
