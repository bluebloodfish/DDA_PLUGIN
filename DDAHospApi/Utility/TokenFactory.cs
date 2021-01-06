using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace DDAApi.Utility
{
    public static class TokenFactory
    {
        public static string GetAppId()
        {
            return Guid.NewGuid().ToString();
        }

        public static string GetSecretKey()
        {
            using (var cryptoProvider = new RNGCryptoServiceProvider())
            {
                byte[] secretKeyByteArray = new byte[32]; //256 bit
                cryptoProvider.GetBytes(secretKeyByteArray);
                return Convert.ToBase64String(secretKeyByteArray);
            }
        }

        public static string GenerateErrorId()
        {
            return $"Err-{ Guid.NewGuid().ToString()}";
        }
    }
}
