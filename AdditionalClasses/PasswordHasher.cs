using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

public class PasswordHasher
{
    //private static readonly byte[] Key = Encoding.UTF8.GetBytes("1234567890123456"); // 16 байт (128 бит) ключ
    //private static readonly byte[] IV = Encoding.UTF8.GetBytes("1234567890123456"); // 16 байт (128 бит) IV

    //public static string Encrypt(string plainText)
    //{
    //    using (Aes aes = Aes.Create())
    //    {
    //        aes.Key = Key;
    //        aes.IV = IV;

    //        using (MemoryStream ms = new MemoryStream())
    //        {
    //            using (CryptoStream cs = new CryptoStream(ms, aes.CreateEncryptor(), CryptoStreamMode.Write))
    //            {
    //                using (StreamWriter sw = new StreamWriter(cs))
    //                {
    //                    sw.Write(plainText);
    //                }
    //            }
    //            return Convert.ToBase64String(ms.ToArray());
    //        }
    //    }
    //}

    //public static string Decrypt(string cipherText)
    //{
    //    using (Aes aes = Aes.Create())
    //    {
    //        aes.Key = Key;
    //        aes.IV = IV;

    //        using (MemoryStream ms = new MemoryStream(Convert.FromBase64String(cipherText)))
    //        {
    //            using (CryptoStream cs = new CryptoStream(ms, aes.CreateDecryptor(), CryptoStreamMode.Read))
    //            {
    //                using (StreamReader sr = new StreamReader(cs))
    //                {
    //                    return sr.ReadToEnd();
    //                }
    //            }
    //        }
    //    }
    //}
}
