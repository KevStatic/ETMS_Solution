using System.Security.Cryptography;
using System.Text;
using ETMS.Application.Interfaces;

namespace ETMS.Infrastructure.Services
{
    public class UrlEncryptionService : IUrlEncryptionService
    {
        private readonly byte[] _key;

        public UrlEncryptionService()
        {
            var keyString = Environment.GetEnvironmentVariable("ENCRYPTION_KEY")
                ?? throw new InvalidOperationException(
                    "ENCRYPTION_KEY environment variable not set. Check your .env file.");

            _key = Encoding.UTF8.GetBytes(keyString.PadRight(32).Substring(0, 32));
        }

        public string Encrypt(int id)
        {
            using var aes = Aes.Create();
            aes.Key = _key;
            aes.GenerateIV();

            using var encryptor = aes.CreateEncryptor();
            var plainText = Encoding.UTF8.GetBytes(id.ToString());
            var encrypted = encryptor.TransformFinalBlock(plainText, 0, plainText.Length);

            var combined = new byte[aes.IV.Length + encrypted.Length];
            Buffer.BlockCopy(aes.IV, 0, combined, 0, aes.IV.Length);
            Buffer.BlockCopy(encrypted, 0, combined, aes.IV.Length, encrypted.Length);

            return Convert.ToBase64String(combined)
                .Replace("+", "-")
                .Replace("/", "_")
                .Replace("=", "");
        }

        public int Decrypt(string encryptedId)
        {
            try
            {
                var base64 = encryptedId
                    .Replace("-", "+")
                    .Replace("_", "/");

                switch (base64.Length % 4)
                {
                    case 2: base64 += "=="; break;
                    case 3: base64 += "="; break;
                }

                var combined = Convert.FromBase64String(base64);

                using var aes = Aes.Create();
                aes.Key = _key;

                var iv = new byte[16];
                var encrypted = new byte[combined.Length - 16];
                Buffer.BlockCopy(combined, 0, iv, 0, 16);
                Buffer.BlockCopy(combined, 16, encrypted, 0, encrypted.Length);

                aes.IV = iv;
                using var decryptor = aes.CreateDecryptor();
                var plainText = decryptor.TransformFinalBlock(encrypted, 0, encrypted.Length);
                return int.Parse(Encoding.UTF8.GetString(plainText));
            }
            catch
            {
                return -1;
            }
        }
    }
}