using System.Security.Cryptography;
using System.Text;

namespace AuthService.Core.Utils
{
    public static class PasswordHelper
    {
        public static string GenerateSalt(int size = 32)
        {
            var saltBytes = new byte[size];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(saltBytes);
            return Convert.ToBase64String(saltBytes);
        }
        public static string HashPassword(string password, string salt)
        {
            var key = Encoding.UTF8.GetBytes(salt);
            using var hmac = new HMACSHA512(key);
            var passwordBytes = Encoding.UTF8.GetBytes(password);
            var hashedBytes = hmac.ComputeHash(passwordBytes);
            return Convert.ToBase64String(hashedBytes);
        }

        public static bool VerifyPassword(string inputPassword, string storedHash, string salt)
        {
            var inputHash = HashPassword(inputPassword, salt);
            return storedHash == inputHash;
        }
    }
}
