using System.Security.Cryptography;
using System.Text;

namespace BankUPG.API.Services
{
    public class PasswordService
    {
        private const int SaltSize = 16;
        private const int HashSize = 32;
        private const int Iterations = 10000;

        public (string hash, string salt) HashPassword(string password)
        {
            var salt = GenerateSalt();
            var hash = HashPasswordWithSalt(password, salt);
            return (Convert.ToBase64String(hash), Convert.ToBase64String(salt));
        }

        public bool VerifyPassword(string password, string storedHash, string storedSalt)
        {
            var saltBytes = Convert.FromBase64String(storedSalt);
            var hashBytes = Convert.FromBase64String(storedHash);
            var computedHash = HashPasswordWithSalt(password, saltBytes);

            return SlowEquals(hashBytes, computedHash);
        }

        private byte[] GenerateSalt()
        {
            using var rng = RandomNumberGenerator.Create();
            var salt = new byte[SaltSize];
            rng.GetBytes(salt);
            return salt;
        }

        private byte[] HashPasswordWithSalt(string password, byte[] salt)
        {
            using var pbkdf2 = new Rfc2898DeriveBytes(password, salt, Iterations, HashAlgorithmName.SHA256);
            return pbkdf2.GetBytes(HashSize);
        }

        private bool SlowEquals(byte[] a, byte[] b)
        {
            var diff = (uint)a.Length ^ (uint)b.Length;
            for (int i = 0; i < a.Length && i < b.Length; i++)
            {
                diff |= (uint)(a[i] ^ b[i]);
            }
            return diff == 0;
        }
    }
}
