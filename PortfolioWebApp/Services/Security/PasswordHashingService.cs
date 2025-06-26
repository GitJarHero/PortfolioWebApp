using System.Security.Cryptography;

namespace PortfolioWebApp.Services {
    
    public interface IPasswordHashingService {
        string Hash(string plainText);
        bool Verify(string plainText, string hash);
    }

    public class PasswordHashingService : IPasswordHashingService
    {
        private const int SaltSize = 16; // 128-bit salt
        private const int HashSize = 32; // 256-bit hash
        private const int Iterations = 100000;

        public string Hash(string plainText)
        {
            byte[] salt = new byte[SaltSize];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(salt);
            }

            return HashWithSalt(plainText, salt);
        }

        public bool Verify(string plainText, string hashedText) {
            
            // Extract salt from stored hash
            byte[] hashBytes = Convert.FromBase64String(hashedText);
            byte[] salt = new byte[SaltSize];
            Array.Copy(hashBytes, 0, salt, 0, SaltSize);

            // Hash the input password with the extracted salt
            string newHash = HashWithSalt(plainText, salt);
        
            return hashedText == newHash;
        }

        private string HashWithSalt(string plainText, byte[] salt) {
            // Create hash with given salt
            byte[] hash;
            using (var pbkdf2 = new Rfc2898DeriveBytes(
                       plainText,
                       salt,
                       Iterations,
                       HashAlgorithmName.SHA256))
            {
                hash = pbkdf2.GetBytes(HashSize);
            }

            // Combine salt and hash
            byte[] hashBytes = new byte[SaltSize + HashSize];
            Array.Copy(salt, 0, hashBytes, 0, SaltSize);
            Array.Copy(hash, 0, hashBytes, SaltSize, HashSize);

            return Convert.ToBase64String(hashBytes);
        }
    }
}