using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace LoginForm
{
    class AES
    {
        private static byte[] key = Encoding.UTF8.GetBytes("4501104175                      ");
        private static string getString(byte[] b)
        {
            return Encoding.UTF8.GetString(b);
        }

        public static byte[] Encrypt(byte[] plainText)
        {

            byte[] data = plainText;
            using (AesCryptoServiceProvider csp = CreateProvider())
            {
                ICryptoTransform encrypter = csp.CreateEncryptor();
                byte[] res = encrypter.TransformFinalBlock(data, 0, data.Length);
                return (res);
            }


        }

        public static byte[] Decrypt(byte[] cipherText)
        {
            byte[] data = cipherText;
            using (AesCryptoServiceProvider csp = CreateProvider())
            {
                ICryptoTransform decrypter = csp.CreateDecryptor();
                byte[] res = decrypter.TransformFinalBlock(data, 0, data.Length);
                return (res);
            }
        }

        private static AesCryptoServiceProvider CreateProvider()
        {
            return new AesCryptoServiceProvider
            {
                KeySize = 256,
                BlockSize = 128,
                Key = key,
                Padding = PaddingMode.PKCS7,
                Mode = CipherMode.ECB,
                IV = new byte[16] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
            };
        }
    }
}
