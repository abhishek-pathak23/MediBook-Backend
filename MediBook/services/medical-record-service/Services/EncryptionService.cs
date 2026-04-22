using System.Security.Cryptography;
using System.Text;

namespace medical_record_service.Services
{
    /// <summary>
    /// AES-256-CBC encryption service for HIPAA-compliant data-at-rest protection.
    /// Encrypts sensitive fields (Diagnosis, Prescription, Notes) before database storage.
    /// </summary>
    public class EncryptionService
    {
        private readonly byte[] _key;

        public EncryptionService(IConfiguration config)
        {
            var keyString = config["Encryption:Key"]
                ?? throw new InvalidOperationException("Encryption key is not configured.");

            // Ensure the key is exactly 32 bytes (256 bits) for AES-256
            _key = Encoding.UTF8.GetBytes(keyString.PadRight(32).Substring(0, 32));
        }

        /// <summary>
        /// Encrypts a plaintext string using AES-256-CBC.
        /// Returns a Base64-encoded string containing [IV + CipherText].
        /// </summary>
        public string Encrypt(string plainText)
        {
            if (string.IsNullOrEmpty(plainText)) return plainText;

            using var aes = Aes.Create();
            aes.Key = _key;
            aes.GenerateIV();

            using var encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
            var plainBytes = Encoding.UTF8.GetBytes(plainText);
            var cipherBytes = encryptor.TransformFinalBlock(plainBytes, 0, plainBytes.Length);

            // Prepend the IV to the cipher text so we can decrypt later
            var result = new byte[aes.IV.Length + cipherBytes.Length];
            Buffer.BlockCopy(aes.IV, 0, result, 0, aes.IV.Length);
            Buffer.BlockCopy(cipherBytes, 0, result, aes.IV.Length, cipherBytes.Length);

            return Convert.ToBase64String(result);
        }

        /// <summary>
        /// Decrypts a Base64-encoded string that was encrypted with Encrypt().
        /// Extracts the IV from the first 16 bytes and decrypts the remainder.
        /// </summary>
        public string Decrypt(string cipherText)
        {
            if (string.IsNullOrEmpty(cipherText)) return cipherText;

            try
            {
                var fullCipher = Convert.FromBase64String(cipherText);

                // Extract IV (first 16 bytes) and cipher data
                var iv = new byte[16];
                var cipher = new byte[fullCipher.Length - 16];
                Buffer.BlockCopy(fullCipher, 0, iv, 0, iv.Length);
                Buffer.BlockCopy(fullCipher, iv.Length, cipher, 0, cipher.Length);

                using var aes = Aes.Create();
                aes.Key = _key;
                aes.IV = iv;

                using var decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
                var plainBytes = decryptor.TransformFinalBlock(cipher, 0, cipher.Length);

                return Encoding.UTF8.GetString(plainBytes);
            }
            catch
            {
                // If decryption fails (e.g., data was stored unencrypted), return as-is
                return cipherText;
            }
        }
    }
}
