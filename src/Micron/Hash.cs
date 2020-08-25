namespace Micron
{
    using System;
    using System.Security.Cryptography;
    using System.Text;

    public class Hash
    {
        public static byte[] CreateHash(string value)
        {
            using var sha256Hash = SHA256.Create();
            return sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(value));
        }

        public static string CreateHashString(string value, Func<byte[], string> getString) =>
            getString(CreateHash(value));

        public static string CreateBase64HashString(string value) =>
            CreateHashString(value, b => Convert.ToBase64String(b));

        public static string CreateHexadecimalHashString(string value) =>
            CreateHashString(value, b => BitConverter.ToString(b).Replace("-", ""));
    }
}
