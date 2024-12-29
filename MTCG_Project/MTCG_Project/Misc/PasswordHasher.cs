using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;

namespace MTCG_Project.Misc
{
    public class PasswordHasher
    {
        // Define constants for hash settings
        private const int SaltSize = 16; // 128-bit salt
        private const int KeySize = 32; // 256-bit key
        private const int Iterations = 10000; // Number of iterations

        public static string HashPassword(string password)
        {
            // Generate a random salt
            using (var rng = RandomNumberGenerator.Create())
            {
                byte[] salt = new byte[SaltSize];
                rng.GetBytes(salt);

                // Derive a key using PBKDF2
                using (var pbkdf2 = new Rfc2898DeriveBytes(password, salt, Iterations, HashAlgorithmName.SHA256))
                {
                    byte[] hash = pbkdf2.GetBytes(KeySize);

                    // Combine salt and hash and return as Base64 string
                    byte[] hashBytes = new byte[salt.Length + hash.Length];
                    Buffer.BlockCopy(salt, 0, hashBytes, 0, salt.Length);
                    Buffer.BlockCopy(hash, 0, hashBytes, salt.Length, hash.Length);

                    return Convert.ToBase64String(hashBytes);
                }
            }
        }

        public static bool VerifyPassword(string password, string hashedPassword)
        {
            // Decode the Base64 string to get the salt and hash
            byte[] hashBytes = Convert.FromBase64String(hashedPassword);

            byte[] salt = new byte[SaltSize];
            Buffer.BlockCopy(hashBytes, 0, salt, 0, SaltSize);

            byte[] originalHash = new byte[KeySize];
            Buffer.BlockCopy(hashBytes, SaltSize, originalHash, 0, KeySize);

            // Derive a key using the provided password and the extracted salt
            using (var pbkdf2 = new Rfc2898DeriveBytes(password, salt, Iterations, HashAlgorithmName.SHA256))
            {
                byte[] testHash = pbkdf2.GetBytes(KeySize);

                // Compare the original hash with the newly computed hash
                return CryptographicOperations.FixedTimeEquals(originalHash, testHash);
            }
        }
    }
}
